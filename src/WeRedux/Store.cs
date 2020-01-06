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
        private readonly Subject<TState> _onReduced = new Subject<TState>();
        private readonly Subject<TState> _onTimeTraveled = new Subject<TState>();
        private readonly Subject<TState> _onChanged = new Subject<TState>();
        private readonly IMapper _mapper;
        private readonly List<IReducer<TState, TAction>> reducers = new List<IReducer<TState, TAction>>();
        #endregion

        #region ctor
        public Store() : this(new Assembly[] { }) { }
        public Store(params Assembly[] assemblies)
        {
            List<Assembly> _assemblies = new List<Assembly>(assemblies);
            _assemblies.Insert(0, Assembly.GetCallingAssembly());
            this._initialState = this._state = new TState();
            MapperConfiguration cfg = new MapperConfiguration(c =>
            {
                c.AddMaps(_assemblies.ToArray());
            });
            var autoTypes = Assembly.GetCallingAssembly().GetReducedClass<TState, TAction>();
            foreach (var @type in autoTypes)
            {
                AddReducer(@type.Type);
            }
            _mapper = cfg.CreateMapper();
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

        }

        public Store(Profile profile) : this(new TState(), profile)
        {

        }

        protected virtual void Initialization()
        {
            OnTimeTraveled.Subscribe(state =>
            {
                State = state;
            });
        }
        #endregion

        #region Public Properties
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
        #endregion

        #region Events
        public IObservable<TState> OnChanged => _onChanged.AsObservable();
        public IObservable<TState> OnReduced => _onReduced.AsObservable();
        public IObservable<TState> OnTimeTraveled => _onTimeTraveled.AsObservable();
        public void TimeTravel(TState state) => _onTimeTraveled.OnNext(state);
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
        public IObservable<IActionState<TState, TAction>> On<T>()
            where T : TAction
        {
            Subject<IActionState<TState, TAction>> obs = new Subject<IActionState<TState, TAction>>();
            ObservableReducer[typeof(T).Name.ToUpperInvariant()] = obs;

            return obs.AsObservable();
        }
        #endregion

        #region Dispatcher
        public void Dispatch(TAction action)
        {
            lock (_syncRoot)
            {
                TState newState = Mapper.Map<TState>(State);
                //State = _reducer(State, action);
                //reducer.Reduce(State, action);
                foreach (var reducer in reducers)
                {
                    reducer.Reduce(newState, State, action);
                }
                if (ObservableReducer.ContainsKey(action.GetName()))
                    ObservableReducer[action.GetName()].OnNext(new ActionState<TState, TAction>() { NewState = newState, State = State, Action = action });
                State = newState;
                _onReduced.OnNext(State);
                
            }
        }
        public void Dispatch<T>() where T : TAction, new()
        {
            this.Dispatch(new T());
        }

        public void Dispatch(string action)
        {
            throw new NotImplementedException();
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
            _onTimeTraveled.Dispose();
        }
        #endregion



    }
}
