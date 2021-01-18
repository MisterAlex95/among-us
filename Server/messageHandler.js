const uuid = require('uuid');
const { clients, rooms } = require("./clients");
const cron = require("node-cron");

// THIS IS UGLY AF SORRY
let _server;

cron.schedule("*/5 * * * * *", function () {
    const now = Date.now();

    Object.keys(clients).forEach((clientId) => {
        const client = clients[clientId];

        if (client && client.lastMsg && client.lastMsg < now - (10 * 1000)) {
            if (client.roomId) {
                const currentRoom = rooms[client.roomId];

                if (currentRoom) {
                    const position = currentRoom.players.indexOf(clientId);

                    if (position >= 0) {
                        // Let's remove the player from the room
                        currentRoom.players.splice(position, 1);
                        if (currentRoom.players.length === 0) {
                            // The room is empty, let's delete it
                            delete rooms[client.roomId];
                            console.log(`Room [${client.roomId}] deleted !`);
                        } else {
                            // Send to each clients to disconnect
                            const clientData = { uuid: clientId };

                            currentRoom.players.forEach(cId => {
                                _server.send("disconnect_" + JSON.stringify(clientData), clients[cId].port, clients[cId].address, function (error) {
                                    if (error) {
                                        client.close();
                                    } else {
                                        console.log(`[${clientId}] Send disconnect !`);
                                    }
                                });
                            });
                        }
                    }
                }
            }

            delete clients[clientId];
            console.log(`[${clientId}] disconnected`);
        }
    })
});

const handle = (data, server) => {
    _server = server;
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
        case "ping":
            return pong(data, server);
        case "switch-privacity":
            return switchPrivacity(data, server);
        case "launch-game":
            return launchGame(data, server);
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
    clients[clientId].lastMsg = Date.now();
    const roomId = makeId(5);
    const room = {
        id: roomId,
        maxPlayers: data.maxPlayers,
        imposters: data.imposters,
        admin: clientId,
        players: [],
        isPrivate: data.isPrivate,
        isRunning: false
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
        rooms: []
    };

    clients[clientId].lastMsg = Date.now();

    Object.keys(rooms).forEach(roomId => {
        const room = rooms[roomId];

        if (!room.isPrivate && !room.isRunning) {
            listRoomAnswer.rooms.push({
                id: roomId,
                nbrPlayer: room.players.length,
                maxPlayers: room.maxPlayers,
                imposters: room.imposters
            });
        }
    });

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
    clients[clientId].lastMsg = Date.now();
    clients[clientId].roomId = data.roomId;
    clients[clientId].position = {
        x: 0,
        y: 0,
        z: 0
    };
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
    } else if (currentRoom.isRunning) {
        const answer = { code: 401 };
        server.send("error_" + JSON.stringify(answer), clients[clientId].port, clients[clientId].address, function (error) {
            if (error) {
                client.close();
            } else {
                console.log(`[${clientId}] Error room running !`);
            }
        });
    } else {
        const randomColor = Math.floor(Math.random() * 16777215).toString(16);

        //Register the color of the client
        clients[clientId].color = randomColor;

        // Add client to the room
        currentRoom.players.push(clientId);

        const answer = { id: data.roomId, color: randomColor, maxPlayers: currentRoom.maxPlayers, imposters: currentRoom.imposters, admin: currentRoom.admin, isPrivate: currentRoom.isPrivate };
        server.send("join-room_" + JSON.stringify(answer), clients[clientId].port, clients[clientId].address, function (error) {
            if (error) {
                client.close();
            } else {
                // Send each client to the new one
                currentRoom.players.forEach(cId => {
                    if (!clients[cId] || !clients[cId].position) return;

                    if (cId !== clientId) {
                        const clientData = { uuid: cId, color: clients[cId].color, position: { x: clients[cId].position.x, y: clients[cId].position.y, z: 0 } }
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
        lastMsg: Date.now()
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

    if (!room) return;

    clients[clientId].lastMsg = Date.now();
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

const pong = (data, server) => {
    const clientId = data.uuid;
    const client = clients[clientId];
    if (client) {
        client.lastMsg = Date.now();
    }
}

const switchPrivacity = (data, server) => {
    const clientId = data.uuid;
    const room = rooms[data.roomId];

    clients[clientId].lastMsg = Date.now();
    if (room.admin === clientId) {
        room.isPrivate = data.isPrivate;
    }
}

const launchGame = (data, server) => {
    const clientId = data.uuid;
    const room = rooms[data.roomId];
    room.isRunning = true;
    clients[clientId].lastMsg = Date.now();

    // Designed imposters
    const impostorsIds = [];
    const maxImposter = Math.min(room.imposters, room.players.length);

    while (impostorsIds.length < maxImposter) {
        const imposterId = room.players[Math.floor(Math.random() * Math.floor(room.players.length))];
        if (!impostorsIds.includes(imposterId)) {
            impostorsIds.push(imposterId);
        }
    }
    room.impostorsIds = impostorsIds;

    room.players.forEach(cId => {
        server.send("launch-game_" + JSON.stringify({ imposter: impostorsIds.includes(cId) }), clients[cId].port, clients[cId].address, function (error) {
            if (error) {
                client.close();
            } else {
                console.log(`[${clientId}] Broadcasted to ${cId} !`);
            }
        });
    });
}

module.exports = {
    handle
};
