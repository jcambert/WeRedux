using AutoMapper;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reflection;
using System.Reactive.Linq;
namespace WeRedux
{
    public class Store<TState, TAction> : IStore<TState, TAction>
        where TState : new()
        where TAction : IAction
    {
        #region private variables
        private TState _state;
        private readonly object _syncRoot = new object();
        private readonly TState _initialState;
        private readonly Subject<TState> _onInitialStateChanged = new Subject<TState>();
        private readonly Subject<TState> _onReduced = new Subject<TState>();
        private readonly Subject<bool> _onTimeTravel = new Subject<bool>();
        private readonly Subject<TState> _onChanged = new Subject<TState>();
        private readonly Subject<IActionState<TState, TAction>> _onAdd = new Subject<IActionState<TState, TAction>>();
        private readonly Subject<string> _onMutation = new Subject<string>();
        private readonly IMapper _mapper;
        private readonly List<IReducer<TState, TAction>> reducers = new List<IReducer<TState, TAction>>();
        private readonly List<HistoricEntry<TState, TAction>> _history = new List<HistoricEntry<TState, TAction>>();
        #endregion

        #region ctor
        public Store()
        {
            Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
            this._initialState = this._state = new TState();
            MapperConfiguration cfg = new MapperConfiguration(c =>
            {
                c.AddMaps(_assemblies);
            });
            var autoTypes = _assemblies.GetReducedClass<TState, TAction>();
            foreach (var @type in autoTypes)
            {
                AddReducer(@type.Type);
            }

            _mapper = cfg.CreateMapper();
            _onInitialStateChanged.OnNext(_initialState);
            Initialization();
        }
        public Store(TState initialState, Profile profile)
        {
            this._initialState = this._state = initialState;
            MapperConfiguration cfg = new MapperConfiguration(c =>
            {
                if (profile != null)
                    c.AddProfile(profile);
            });
            _mapper = cfg.CreateMapper();
            _onInitialStateChanged.OnNext(_initialState);
            Initialization();
        }

        public Store(Profile profile) : this(new TState(), profile)
        {

        }

        protected virtual void Initialization()
        {
            History.Add(new HistoricEntry<TState, TAction>(new ActionState<TState, TAction> { NewState = _initialState, State = _initialState }));
            OnMutation.Subscribe(o =>
            {
                CurrentMutation = o;
            });
            OnAdd.Subscribe(o =>
            {
                History.Add(new HistoricEntry<TState, TAction>(o));
            });
        }
        #endregion

        #region Public Properties
        public String CurrentMutation { get; private set; }
        public TState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                _onChanged.OnNext(_state);
            }
        }

        public TState Initial => _initialState;
        protected IMapper Mapper => _mapper;

        public IList<HistoricEntry<TState, TAction>> History => _history;
        #endregion

        #region Events
        public IObservable<TState> OnInitialStateChanged => _onInitialStateChanged.AsObservable();
        public IObservable<TState> OnChanged => _onChanged.AsObservable();
        public IObservable<IActionState<TState, TAction>> OnAdd => _onAdd.AsObservable();
        public IObservable<TState> OnReduced => _onReduced.AsObservable();
        public IObservable<bool> OnTimeTravel => _onTimeTravel.AsObservable();
        public IObservable<string> OnMutation => _onMutation.AsObservable();
        //public void TimeTravel(TState state) => _onTimeTraveled.OnNext(state);
        #endregion

        #region Reducer
        public void AddReducer<TReducer>()
            where TReducer : Reducer<TState, TAction>, new()
        {
            this.AddReducer<TReducer>(new TReducer());
        }


        public void AddReducer<TReducer>(TReducer reducer)
            where TReducer : Reducer<TState, TAction>
        {

            reducer.OnReduce.Subscribe(o =>
            {
                reducer.Execute(o);
            });
            reducers.Add(reducer);
        }

        private void AddReducer(Type type)
        {
            this.AddReducer(type.GetConstructor(new Type[] { }).Invoke(null) as Reducer<TState, TAction>);
        }

        public Dictionary<string, Subject<IActionState<TState, TAction>>> ObservableReducer { get; } = new Dictionary<string, Subject<IActionState<TState, TAction>>>();
        public Dictionary<string, Type> ActionByName { get; } = new Dictionary<string, Type>();

        public IObservable<IActionState<TState, TAction>> On<T>()
            where T : TAction
        {
            Subject<IActionState<TState, TAction>> obs = new Subject<IActionState<TState, TAction>>();
            var key = typeof(T).Name.ToUpperInvariant();
            ObservableReducer[key] = obs;
            ActionByName[key] = typeof(T);
            return obs.AsObservable();
        }
        #endregion

        #region Dispatcher
        public void Dispatch(TAction action)
        {
            lock (_syncRoot)
            {
                _onMutation.OnNext(action.GetType().Name.ToUpperInvariant());
                TState newState = Mapper.Map<TState,TState>(State,o=> {  o.CreateInstance<TState>(); });

                foreach (var reducer in reducers)
                {
                    reducer.Reduce(newState, State, action);
                }
                
                if (ObservableReducer.ContainsKey(action.GetName()))
                    ObservableReducer[action.GetName()].OnNext(new ActionState<TState, TAction>() { NewState = newState, State = State, Action = action });
                State = newState;
                _onAdd.OnNext(new ActionState<TState, TAction>() { NewState = newState, State = State, Action = action });
                _onReduced.OnNext(State);

            }
        }
        public void Dispatch<T>() where T : TAction, new()
        {
            this.Dispatch(new T());
        }

        public void Dispatch(string action)
        {
            var key = action.Trim().ToUpperInvariant();
            if (ActionByName.ContainsKey(key))
            {
                Dispatch((TAction)ActionByName[key].GetConstructor(new Type[] { }).Invoke(null));
            }
            //throw new NotImplementedException();
        }
        #endregion

        #region IDisposable

#pragma warning disable CA1063 // Implement IDisposable Correctly
        public void Dispose()
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            foreach (var reducer in reducers)
            {
                reducer.Dispose();
            }
            _onReduced.Dispose();
            _onChanged.Dispose();
            _onAdd.Dispose();
            _onTimeTravel.Dispose();
            _onMutation.Dispose();
        }

        #endregion


        #region Travel
        public void TravelTo(int index)
        {
            _onTimeTravel.OnNext(true);
            var hstate = History[index];
            State = hstate.ActionState.NewState;
            _onTimeTravel.OnNext(false);

        }
        #endregion

    }
}
