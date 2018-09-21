Vue.use(VueTables.ClientTable);

const apiBaseUrl = 'https://jehollan-inventorymanager.azurewebsites.net';
//const apiBaseUrl = 'http://localhost:7071';

var inventoryColumns = ['Product', 'Seattle', 'Orlando', 'London', 'Tokyo'];

var vm = new Vue({
    el: "#app",
    data: {
        columns: inventoryColumns,
        data: { inventory: [] },
        options: {

        },
        buttonText: "Connect to Azure Functions",
        isConnected: false
    },
    methods: {
        connectToSignalR: function (event) {
            connectToSignalR();
        }
    }
});

function getConnectionInfo() {
    return axios.post(`${apiBaseUrl}/api/negotiate`)
        .then(resp => resp.data);
}

function arrayUnique(array) {
    var a = array.concat();
    for (var i = 0; i < a.length; ++i) {
        for (var j = i + 1; j < a.length; ++j) {
            if (a[i] === a[j])
                a.splice(j--, 1);
        }
    }

    return a;
}

function connectToSignalR() {
    vm.buttonText = "Connecting...";
    getConnectionInfo().then(info => {
        const options = {
            accessTokenFactory: () => info.accessToken
        };
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(info.url, options)
            .configureLogging(signalR.LogLevel.Information)
            .build();
        connection.on('inventory', (message) => {
            console.log(message);
            var m = JSON.parse(message);
            var index = vm.data.inventory.findIndex(obj => {
                return obj.Product == m.Product
            });

            console.log(`row: ${JSON.stringify(vm.data.inventory[index])}`)
            var proxy = JSON.parse(JSON.stringify(vm.data.inventory[index]));
            TweenLite.to(proxy, 1,
                {
                    ...m,
                    onUpdate: updateInventory,
                    onUpdateParams: [index, proxy],
                    onComplete: () => { console.log('animation complete')}
                });
            vm.columns = arrayUnique(vm.columns.concat(Object.keys(m)));
            console.log('set\n' + index);
        });
        connection.onclose(() => console.log('disconnected'));
        console.log('connecting...');
        connection.start()
            .then(() => {
                console.log('connected!');
                vm.buttonText = "Connected";
                vm.isConnected = true;
            })
            .catch(console.error);
    }).catch(alert);
    
}

function updateInventory(index, obj) {
    Object.keys(obj).forEach(function (key) { if (typeof obj[key] == 'number') { obj[key] = obj[key].toFixed(0) }});
    vm.$set(vm.data.inventory, index, obj);
}

function fetchInventory() {
    return axios.get(`${apiBaseUrl}/api/inventory`)
        .then(resp => resp.data);
}

(
    function () {
        console.log('calling inventory...');
        fetchInventory().then(inventory => {
            vm.data.inventory = inventory;
            console.log('inventory set');
        });
    }
)();

