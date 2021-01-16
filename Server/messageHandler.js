const uuid = require('uuid');
const clients = require("./clients");

const handle = (data, server) => {
    switch (data.type) {
        case "handcheck":
            return handcheck(data, server);
        case "movement":
            return movement(data, server);
        default:
            return {};
    }
}

// Receive: JSON_{"type":"handcheck"}
// Answer Own: { uuid: 'ff6630bd-0328-4377-adac-f7eb54bc5c54' }
// Answer All: { new connexion + color }
const handcheck = (data, server) => {
    // Generate clientId and color of the client
    const clientId = uuid.v4();
    const randomColor = Math.floor(Math.random() * 16777215).toString(16);

    //Register a new client
    clients[clientId] = {
        address: data.address,
        port: data.port,
        color: randomColor,
    };

    // Send uuid to the client
    server.send("handcheck_" + JSON.stringify({ uuid: clientId, color: randomColor }), clients[clientId].port, clients[clientId].address, function (error) {
        if (error) {
            client.close();
        } else {
            console.log(`[${clientId}] Created !`);
            Object.keys(clients).forEach(cId => {
                if (cId !== clientId) {
                    server.send("connexion_" + JSON.stringify({ uuid: clientId, color: randomColor }), clients[cId].port, clients[cId].address, function (error) {
                        if (error) {
                            client.close();
                        } else {
                            console.log(`[${clientId}] Broadcasted to ${cId} !`);
                        }
                    });
                }
            });
        }
    });
}

const movement = (data, server) => {
    const clientId = data.uuid;

    Object.keys(clients).forEach(cId => {
        if (cId !== clientId) {
            server.send("position_" + JSON.stringify({ uuid: clientId, position: { x: data.position.x, y: data.position.y, z: 0 } }), clients[cId].port, clients[cId].address, function (error) {
                if (error) {
                    client.close();
                } else {
                    console.log(`[${clientId}] Broadcasted to ${cId} !`);
                }
            });
        }
    });
}

const client = {
    handle
};

module.exports = client;
