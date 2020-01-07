using AutoMapper;
using System;
using System.Linq;
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
        private TState _initialState;
        private TState _state;
        private readonly object _syncRoot = new object();
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
            //this._initialState = this._state = new TState();
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
            //_onInitialStateChanged.OnNext(_initialState);
            Initialization();
        }
        public Store(Profile profile)
        {
            //this._initialState = this._state = new TState();
            MapperConfiguration cfg = new MapperConfiguration(c =>
            {
                if (profile != null)
                    c.AddProfile(profile);
            });
            _mapper = cfg.CreateMapper();
            //_onInitialStateChanged.OnNext(_initialState);
            Initialization();
        }
        public Store(Action<IMapperConfigurationExpression> map)
        {
            //this._initialState = this._state = new TState();
            MapperConfiguration cfg = new MapperConfiguration(map);
            _mapper = cfg.CreateMapper();
            //_onInitialStateChanged.OnNext(_initialState);
            Initialization();
        }
        /*  public Store(Profile profile) : this(new TState(), profile)
          {

          }*/

        protected virtual void Initialization()
        {

            OnInitialStateChanged.Subscribe(state =>
            {
                History.Clear();
                History.Add(new HistoricEntry<TState, TAction>(new ActionState<TState, TAction> { NewState = state, State = state }));
            });
            OnMutation.Subscribe(o =>
            {
                CurrentMutation = o;
            });
            OnAdd.Subscribe(o =>
            {
                History.Add(new HistoricEntry<TState, TAction>(o));
            });
            OnTimeTravel.Subscribe(travel => Travelling = travel);

            this.InitialState = this._state = new TState();
        }
        #endregion

        #region Public Properties
        public String CurrentMutation { get; private set; }
        public bool Travelling { get; private set; } = false;
        public TState State
        {
            get { return _state; }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("State cannot be null");
                _state = value;
                if (!Travelling)
                    _onChanged.OnNext(_state);
            }
        }

        public TState InitialState
        {
            get { return _initialState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Initial State cannot be null");
            _initialState = value;
                _onInitialStateChanged.OnNext(value);
            }
        }

        public TState LastState => History.Last().ActionState.State;
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
                TState newState = Mapper.Map<TState, TState>(LastState, o => { o.CreateInstance<TState>(); });

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

        #region Resetting
        public void Reset()
        {

            this.InitialState = this.State = new TState();

        }
        #endregion

    }
}
