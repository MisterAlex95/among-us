const uuid = require('uuid');
const { clients, rooms } = require("./clients");

const handle = (data, server) => {
    switch (data.type) {
        case "handcheck":
            return handcheck(data, server);
        case "movement":
            return movement(data, server);
        case "create-room":
            return createRoom(data, server);
        case "join-room":
            return joinRoom(data, server);
        case "list-room":
            return listRoom(data, server);
        default:
            return {};
    }
}

// Add a way to create a room with config
// { maxPlayers: int, imposters: int }
const createRoom = (data, server) => {
    const makeId = (length) => {
        return [...(new Array(length))].map(() => Math.floor(Math.random() * 16).toString(16)).join('').toUpperCase();
    }
    const clientId = data.uuid;
    const roomId = makeId(5);
    const room = {
        id: roomId,
        maxPlayers: data.maxPlayers,
        imposters: data.imposters,
        admin: clientId,
        players: []
    };
    rooms[roomId] = room;
    server.send("create-room_" + JSON.stringify(room), clients[clientId].port, clients[clientId].address, function (error) {
        if (error) {
            client.close();
        } else {
            console.log(`[${clientId}] Send new room !`);
        }
    });
}

const listRoom = (data, server) => {
    const clientId = data.uuid;
    const listRoomAnswer = {
        rooms: Object.keys(rooms).map(roomId => {
            const room = rooms[roomId];
            return {
                id: roomId,
                nbrPlayer: room.players.length,
                maxPlayers: room.maxPlayers,
                imposters: room.imposters
            };
        })
    }

    server.send("list-room_" + JSON.stringify(listRoomAnswer), clients[clientId].port, clients[clientId].address, function (error) {
        if (error) {
            client.close();
        } else {
            console.log(`[${clientId}] Created !`);
        }
    });
}

const joinRoom = (data, server) => {
    const clientId = data.uuid;
    const currentRoom = rooms[data.roomId];

    if (!currentRoom) {
        const answer = { code: 404 };
        server.send("error_" + JSON.stringify(answer), clients[clientId].port, clients[clientId].address, function (error) {
            if (error) {
                client.close();
            } else {
                console.log(`[${clientId}] Error room doesn't exist !`);
            }
        });
    } else if (currentRoom.players.length > currentRoom.maxPlayers) {
        const answer = { code: 400 };
        server.send("error_" + JSON.stringify(answer), clients[clientId].port, clients[clientId].address, function (error) {
            if (error) {
                client.close();
            } else {
                console.log(`[${clientId}] Error room full !`);
            }
        });
    } else {
        const randomColor = Math.floor(Math.random() * 16777215).toString(16);

        //Register the color of the client
        clients[clientId].color = randomColor;

        // Add client to the room
        currentRoom.players.push(clientId);

        const answer = { id: data.roomId, color: randomColor };
        server.send("join-room_" + JSON.stringify(answer), clients[clientId].port, clients[clientId].address, function (error) {
            if (error) {
                client.close();
            } else {
                // Send each client to the new one
                currentRoom.players.forEach(cId => {
                    if (cId !== clientId) {
                        const clientData = { uuid: cId, color: clients[cId].color }
                        server.send("connexion_" + JSON.stringify(clientData), clients[clientId].port, clients[clientId].address, function (error) {
                            if (error) {
                                client.close();
                            } else {
                                console.log(`[${clientId}] Send connexion !`);
                            }
                        });
                    }
                });

                // Send connexion of the new client to others clients
                currentRoom.players.forEach(cId => {
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
}

const handcheck = (data, server) => {
    // Generate clientId and color of the client
    const clientId = uuid.v4();

    // Generate initials informations to the client
    const handcheckAnswer = {
        uuid: clientId,
    };

    //Register a new client
    clients[clientId] = {
        address: data.address,
        port: data.port,
    };

    server.send("handcheck_" + JSON.stringify(handcheckAnswer), clients[clientId].port, clients[clientId].address, function (error) {
        if (error) {
            client.close();
        } else {
            console.log(`[${clientId}] Created !`);
        }
    });
}

const movement = (data, server) => {
    const clientId = data.uuid;
    const room = rooms[data.roomId];

    clients[clientId].position = {
        x: data.position.x,
        y: data.position.y,
        z: 0
    };

    room.players.forEach(cId => {
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

module.exports = {
    handle
};
