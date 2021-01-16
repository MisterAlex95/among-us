
const msgParser = (msg) => {
    msg = msg.split('_');
    const type = msg[0];
    const data = msg[1];

    switch (type) {
        case "JSON":
            return jsonParse(data);

        default:
            return {};
    }
}

const jsonParse = (data) => {
    try {
        return JSON.parse(data);
    } catch (error) {
        console.error("jsonParser", error);
    }
    return {}
}

const client = {
    msgParser,
    jsonParse
};

module.exports = client;
