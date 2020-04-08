
/*(function () {*/

    let extension = window.__REDUX_DEVTOOLS_EXTENSION__;
    if (!extension) {
        console.log('Redux DevTools not installed.');
        
    } else {
        console.log('You can play with Redux DevTools Extensions');
    }



if (!extension) {

} else {

}



    
    
    //window.devTools = {};



    
var weredux = {
    subscribe : function (weredux) {
        weredux.devTools.subscribe((message) => {
            console.log(message);
            if (message.type === 'START') {
                console.log('Start with Redux DevTools.');
                weredux.devToolsReady('Connected');

            } else if (message.type === 'DISPATCH' && message.state) {
                // Time-traveling
                weredux.travelTo(message.payload.actionId);
            } else if (message.type === 'DISPATCH' && message.payload) {
                var payload = message.payload;
                if (payload.type === 'IMPORT_STATE') {
                    Object.keys(payload.nextLiftedState.actionsById).forEach(function (key) {
                        var action = payload.nextLiftedState.actionsById[key];
                        if (action.type === "PERFORM_ACTION") {
                            weredux.dispatch(action.action.type);
                        }
                    });
                    weredux.travelTo(payload.nextLiftedState.currentStateIndex);
                }
            }

        });
    },
    init: function (dotnetref, name) {

        let _weredux = {
            connect: function () {
                if (!this.dotnetref) return;

                this.devTools = extension.connect({ name: name + ' State' });
                if (this.devTools) {
                    console.log('Your are connected to Redux DevTools');
                    weredux.subscribe(this);
                    console.log('You are listenning DevTools Events');
                } else {
                    console.log('Unable to connect to Redux DevTools.');
                    return;
                }


            },
            devToolsReady: function (message) {
                if (!this.dotnetref) return;
                // this.dotnetref.invokeMethodAsync('DevToolsReady', 'Connected');
            },
            init: function (state) {
               // console.log('Init Devtools:', state);
                this.devTools.init(state);
            },
            dispatch: function (action) {
                if (!this.dotnetref) return;
                if (action === "@@INIT") {
                    this.dotnetref.invokeMethodAsync('Reset');
                }

                else
                    this.dotnetref.invokeMethodAsync('Dispatch', action);
            },
            travelTo: function (index) {
                //console.log('Travel to:', index);
                this.dotnetref.invokeMethodAsync('TravelTo', index);
            },

            onMutation: function (mutation) {
                this.mutation = mutation;
                //console.log(mutation);
            },
            onChanged: function (mutation, state) {
                //console.log('State changed to:', mutation, ' State', state);
                this.devTools.send(mutation, state);

            }
        }

        _weredux.name = name;
        _weredux.dotnetref = dotnetref;

        return _weredux;
    },
    addStore : function (dotnetref, name, initialState) {
        _name = name.toLowerCase();
        this.stores[_name] = this.init(dotnetref, name);
        this.stores[_name].connect();
        this.stores[_name].init(initialState);
        //console.log("The Store with name:" + name + " was successfully created");
    },
    stores: {}
};


/*}());*/

