
(function () {
    ;
    var extension = window.__REDUX_DEVTOOLS_EXTENSION__;
    if (!extension) {
        console.log('Redux DevTools not installed.');
        return;
    } else {
        console.log('You can play with Redux DevTools Extensions');
    }

    var config = { name: 'Blazor Redux' };




    var devTools = extension.connect(config);
    if (!devTools) {
        console.log('Unable to connect to Redux DevTools.');
        return;
    } else {
        console.log('Your are connected to Redux DevTools');
    }



    var weredux = {

        devToolsReady: function (message) {
            if (!this.dotnetref) return;
            this.dotnetref.invokeMethodAsync('DevToolsReady', 'Connected');
        },
        init: function (state) {
            console.log('Init Devtools:', state);
            window.devTools.init(state);
        },
        dispatch: function (action) {
            if (!this.dotnetref) return;
            this.dotnetref.invokeMethodAsync('Dispatch', action);
        },
        travelTo: function (message) {
            this.dotnetref.invokeMethodAsync('TravelTo', message.payload.actionId);
        },
        onMutation: function (mutation) {
            this.mutation = mutation;
        },
        onChanged: function (state) {
            console.log('State changed to:', state);

            window.devTools.send(this.mutation,state);
        }
    }
    window.setComponent = function (dotnetref) {
        window.weredux.dotnetref=dotnetref;

    }
    window.weredux = weredux;

    devTools.subscribe((message) => {
        console.log(message);
        if (message.type === 'START') {
            console.log('Start with Redux DevTools.');
            window.weredux.devToolsReady('Connected');

        } else if (message.type === 'DISPATCH' && message.state) {
            // Time-traveling
            window.weredux.travelTo(message);
        }

    });
    window.devTools = devTools;






}());