﻿using AutoMapper;
using MicroS_Common.Actions;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;


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
        private readonly Subject<int> _onTravelTo = new Subject<int>();
        private readonly Subject<IMutationstate<TState>> _onChanged = new Subject<IMutationstate<TState>>();
        private readonly IMapper _mapper;
        private readonly List<HistoricEntry<TState, TAction>> _history = new List<HistoricEntry<TState, TAction>>();
        private Subject<TAction> Dispatcher = new Subject<TAction>();
        #endregion

        #region ctor

        public Store(string name, Action<IMapperConfigurationExpression> map)
        {
            Console.WriteLine("Create New Store");
            this.Name = name;
            MapperConfiguration cfg = new MapperConfiguration(map);
            _mapper = cfg.CreateMapper();
            Initialization();


        }


        protected virtual void Initialization()
        {

            OnInitialStateChanged.Subscribe(state =>
            {
                History.Clear();

                Dispatch((TAction)(object)new InitialAction());

            });
            Dispatcher.AsObservable().Where(t => t.GetType() == typeof(InitialAction)).Subscribe(action =>
            {
                StateChanged(action);
            });
           
            this.InitialState = this._state = new TState();
        }
        #endregion

        #region Public Properties
        public string Name { get; private set; }
        public string CurrentMutation { get; private set; }
        public bool Travelling { get; private set; } = false;
        public TState State
        {
            get { return _state; }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("State cannot be null");
                _state = value;
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

        public List<HistoricEntry<TState, TAction>> History => _history;

        public bool IsEmpty => History.Count <= 1;//The Initial State

        public bool IsReady { get; private set; } = false;
        #endregion

        #region Public Methods

        #endregion

        #region Events
        public IObservable<TState> OnInitialStateChanged => _onInitialStateChanged.AsObservable();
        public IObservable<IMutationstate<TState>> OnChanged => _onChanged.AsObservable();
        public IObservable<int> OnTravelTo => _onTravelTo.AsObservable();
        #endregion

        #region Reducer






        private Dictionary<string, Type> ActionByName { get; } = new Dictionary<string, Type>();




        public IObservable<TAction> On<T>()
            where T : TAction
        {
            var key = typeof(T).Name.ToUpperInvariant();
            ActionByName[key] = typeof(T);
            return Dispatcher.AsObservable().Where(t => t.GetType() == typeof(T));
        }
        #endregion

        #region Dispatcher

        public void StateChanged<T>(T action) where T : TAction
        {
            if (!_onChanged.IsDisposed)
            {
               // if (typeof(T).Name == "IAction")
                //    Debugger.Break();
                //Console.WriteLine($"Store:StateChanged {typeof(T).Name}");
                _onChanged.OnNext(new MutationState<TState>() { Mutation = action.GetMutation(), State = State, Action = action.GetType() });
            }
        }
        private void InternalDispatch<T>(T action) where T : TAction
        {
            var mutationQuery = action is IStaticMutation ? ((IStaticMutation)action).Mutation : QueryHelpers.AddQueryString(action.GetType().Name.ToUpperInvariant(), action.ToDictionary());

            if (action is InitialAction)
            {
                History.Add(new HistoricEntry<TState, TAction>(new ActionState<TState, TAction>() { State = State, Action = action }));
                Dispatcher.OnNext(action);
            }
            else
            {
                TState newState = new TState();
                Mapper.Map<TState, TState>(LastState, newState);
                State = newState;
                History.Add(new HistoricEntry<TState, TAction>(new ActionState<TState, TAction>() { State = newState, Action = action }));
                Dispatcher.OnNext(action);
            }
        }
        public Task DispatchAsync<T>(Action<T> action ) where T : TAction,new()
        {
            return Task.Factory.StartNew(() =>
            {
                T t = new T();
                action?.Invoke(t);
                //this.Dispatch(t);
                InternalDispatch<T>(t);
            });
        }
        public void Dispatch<T>(T action) where T : TAction
        {
            lock (_syncRoot)
            {
                var mutationQuery = action is IStaticMutation ? ((IStaticMutation)action).Mutation : QueryHelpers.AddQueryString(action.GetType().Name.ToUpperInvariant(), action.ToDictionary());
              
                if (action is InitialAction)
                {
                    History.Add(new HistoricEntry<TState, TAction>(new ActionState<TState, TAction>() { State = State, Action = action }));
                    Dispatcher.OnNext(action);
                }
                else
                {
                    TState newState = new TState();
                    Mapper.Map<TState, TState>(LastState, newState);
                    State = newState;
                    History.Add(new HistoricEntry<TState, TAction>(new ActionState<TState, TAction>() { State = newState, Action = action }));
                    Dispatcher.OnNext(action);
                }

            }
        }


        public void Dispatch<T>() where T : TAction, new()
        {
            this.Dispatch(new T());
        }

        public void Dispatch<T>(Action<T> action) where T : TAction, new()
        {
            T t = new T();
            action?.Invoke(t);
            this.Dispatch(t);
        }


        public void Dispatch(string action)
        {
            //var key = action.Trim().ToUpperInvariant();
            string key;
            var queries = QueryHelpers.ParseQuery(action);
            var qs = action.ParseQuery(out key);
            if (ActionByName.ContainsKey(key))
            {
                Type type = ActionByName[key];
                TAction _action = (TAction)type.GetConstructor(new Type[] { }).Invoke(null);
                foreach (var q in qs)
                {
                    var method = type.GetProperty(q.Key);
                    var value = Convert.ChangeType(q.Value.FirstOrDefault(), method.PropertyType);
                    method?.GetSetMethod()?.Invoke(_action, new object[] { value });
                }
                Dispatch(_action);
            }
        }

        #endregion

        #region Travel
        public void TravelTo(int index)
        {
            Travelling = true;
            try
            {

                var hstate = History[index];
                State = hstate.ActionState.State;
                _onTravelTo.OnNext(index);
            }
            catch (Exception ex)
            {
#if DEBUG
                //  Debugger.Break();
#endif
            }
            finally
            {
                Travelling = false;
            }



        }

        #endregion

        #region Resetting
        public void Reset()
        {

            this.InitialState = this.State = new TState();

        }
        #endregion
        private string GenerateName() => Guid.NewGuid().ToString();
        public override string ToString()
        {
            return Name;
        }


        #region IDisposable Support
        private bool disposedValue = false; // Pour détecter les appels redondants

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: supprimer l'état managé (objets managés).

                    _onChanged.Dispose();
                    _onTravelTo.Dispose();
                    _onInitialStateChanged.Dispose();
                }

                // TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
                // TODO: définir les champs de grande taille avec la valeur Null.

                disposedValue = true;
            }
        }

        // TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
        // ~StorageBase()
        // {
        //   // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
        //   Dispose(false);
        // }

        // Ce code est ajouté pour implémenter correctement le modèle supprimable.
        public void Dispose()
        {
            // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
            Dispose(true);
            // TODO: supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
            // GC.SuppressFinalize(this);
        }

        










        #endregion


    }
}
