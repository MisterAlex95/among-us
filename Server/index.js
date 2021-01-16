const dgram = require('dgram');

const parser = require('./parser');
const messageHandler = require('./messageHandler');
const clients = require("./clients");

const server = dgram.createSocket('udp4');

server
    .on('listening', () => {
        const address = server.address();
        console.log(`server listening ${address.address}:${address.port}`);
    })
    .on('message', (msg, rinfo) => {
        console.log(`server got: ${msg} from ${rinfo.address}:${rinfo.port}`);

        const obj = parser.msgParser(msg.toString());

        // Add some data at the first connection to the object to save it later
        if (!obj.uuid || !clients[obj.uuid]) {
            obj.address = rinfo.address;
            obj.port = rinfo.port;
        }

        // Handle the message
        messageHandler.handle(obj, server);
    })
    .on('error', (err) => {
        console.log(`server error:\n${err.stack}`);
        server.close();
    });

server.bind(8080);