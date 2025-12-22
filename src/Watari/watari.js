function initWatari(serverPort) {
    window.watari = {
        invoke: async function(method, ...args) {
            const response = await fetch(`http://localhost:${serverPort}/invoke`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ method, args })
            });
            return await response.json();
        },
        _eventHandlers: {},
        on: function(event, handler) {
            if (!this._eventHandlers[event]) {
                this._eventHandlers[event] = [];
            }
            this._eventHandlers[event].push(handler);
        },
        off: function(event, handler) {
            if (this._eventHandlers[event]) {
                this._eventHandlers[event] = this._eventHandlers[event].filter(h => h !== handler);
            }
        },
        _ws: null,
        _connectEvents: function() {
            this._ws = new WebSocket(`ws://localhost:${serverPort}/events`);
            this._ws.onmessage = (event) => {
                const msg = JSON.parse(event.data);
                const handlers = this._eventHandlers[msg.event] || [];
                handlers.forEach(h => h(msg.data));
            };
            this._ws.onclose = () => {
                setTimeout(() => this._connectEvents(), 1000);
            };
        }
    };
    window.watari._connectEvents();
}