var connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

var _connectionId = '';

connection.on("RecieveMessage", function (data) {
    //console.log(data);
    var message = document.createElement("div")
    message.classList.add('message')


    var messageHeader = document.createElement("div")
    messageHeader.classList.add('message-header')
    var header = document.createElement("header")
    header.appendChild(document.createTextNode(data.name))

    var footer = document.createElement("footer")
    footer.appendChild(document.createTextNode(data.timeStamp))

    messageHeader.appendChild(header)
    messageHeader.appendChild(footer)

    var messageBody = document.createElement("div")
    messageBody.classList.add('message-body')
    var p = document.createElement("p")
    p.appendChild(document.createTextNode(data.text))

    messageBody.appendChild(p)

    message.appendChild(messageHeader)
    message.appendChild(messageBody)

    document.querySelector('.chat-body').append(message);

});

var joinRoom = function () {
    var url = '/Chat/JoinRoom/' + _connectionId + '/@Model.Name'
    axios.post(url, null)
        .then(res => {
            console.log("Room Joined!", res);
        })
        .catch(err => {
            console.err("Failed to Join Room!", res)
        })
}

connection.start()
    .then(function () {
        connection.invoke('getConnectionId')
            .then(function (connectionId) {
                _connectionId = connectionId;
                joinRoom();
            })
    })
    .catch(function (err) {
        console.log(err)
    })


var sendMessage = function (event) {
    event.preventDefault();

    var data = new FormData(event.target)
    document.getElementById('message-input').value = '';
    axios.post('/Chat/SendMessage', data)
        .then(res => {
            console.log("Message Sent")
        })
        .catch(err => {
            console.log("Unsuccessful")
        })
}