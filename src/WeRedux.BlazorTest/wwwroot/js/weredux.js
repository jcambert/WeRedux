
(function () {

    var extension = window.__REDUX_DEVTOOLS_EXTENSION__;
    if (!extension) {
        console.log('Redux DevTools not installed.');
        return;
    } else {
        console.log('You can play with Redux DevTools Extensions');
    }

    //var config = { name: 'Blazor Redux' };




    /* var devTools = extension.connect(config);
     if (!devTools) {
         console.log('Unable to connect to Redux DevTools.');
         return;
     } else {
         console.log('Your are connected to Redux DevTools');
     }*/


    var constructor = function (dotnetref, name) {

        var weredux = {
            connect: function () {
                if (!this.dotnetref) return;

                this.devTools = extension.connect({ name: name+' State' });
                if (this.devTools) {
                    console.log('Your are connected to Redux DevTools');
                    subscribe(weredux);
                    console.log('You are listenning DevTools Events');
                } else {
                    console.log('Unable to connect to Redux DevTools.');
                    return;
                }


            },
            devToolsReady: function (message) {
                if (!this.dotnetref) return;
                this.dotnetref.invokeMethodAsync('DevToolsReady', 'Connected');
            },
            init: function (state) {
                console.log('Init Devtools:', state);
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
                this.dotnetref.invokeMethodAsync('TravelTo', index);
            },

            onMutation: function (mutation) {
                this.mutation = mutation;
            },
            onChanged: function (state) {
                console.log('State changed to:', state);

                this.devTools.send(this.mutation, state);
            }
        }

        weredux.name = name;
        weredux.dotnetref = dotnetref;

        return weredux;
    }

    window.addStore = function (dotnetref, name, initialState) {
        _name = name.toLowerCase();
        window.weredux[_name] = constructor(dotnetref, name);
        window.weredux[_name].connect();
        window.weredux[_name].init(initialState);
        console.log("The Store with name:" + name + " was successfully created");
    }

    var subscribe = function (weredux) {
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
    }
     window.weredux = {};
    //window.devTools = {};






}());