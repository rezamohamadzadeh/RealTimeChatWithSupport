var chatterName = 'Visitor';

var dialogEl = document.getElementById('chatDialog');

// Initialize the SignalR client
var connection = new signalR.HubConnectionBuilder()
    .withUrl('/chatHub')
    .build();

connection.on('ReceiveMessage', renderMessage);
connection.on('PassRoomId', PassId);

connection.onclose(function () {
    onDisconnected();
    setTimeout(startConnection, 5000);
})

function startConnection() {
    connection.start()
        .then(onConnected)
        .catch(function (err) {
            console.error(err);
        });
}

function onDisconnected() {
    dialogEl.classList.add('disconnected');
}

function onConnected() {
    dialogEl.classList.remove('disconnected');

    var messageTextboxEl = document.getElementById('messageTextbox');
    messageTextboxEl.focus();

    connection.invoke('SetName', chatterName);
    connection.invoke('GetRoomId');
}



function showChatDialog() {
    dialogEl.style.display = 'block';
}

function sendMessage(text) {
    if (text && text.length) {
        connection.invoke('SendMessage', chatterName, text);
    }
}
function PassId(roomId) {
    var chatFormEl = document.getElementById('fileForm');
    chatFormEl[1].value = roomId;
    chatFormEl[2].value = chatterName;
}

function ready() {
    setTimeout(showChatDialog, 750);

    var chatFormEl = document.getElementById('chatForm');
    chatFormEl.addEventListener('submit', function (e) {
        e.preventDefault();
        var text = e.target[0].value;
        e.target[0].value = '';
        sendMessage(text);
    });


    var welcomePanelEl = document.getElementById('chatWelcomePanel');
    welcomePanelEl.addEventListener('submit', function (e) {
        e.preventDefault();

        var name = e.target[0].value;
        if (name && name.length) {
            welcomePanelEl.style.display = 'none';
            chatterName = name;
            startConnection();
        }
    });
}
function CheckWinFocus() {
    if (document.hasFocus()) return true;
    else return false;

}
function GetNotification(name, time, message) {
    Push.create(`Message from ` + name  , {
        body: name + ' ' + time + "\n" + message,
        data: "SinjulMSBH",
        icon: '',
        link: "",
        tag: '',
        requireInteraction: true,
        timeout: 5000,
        vibrate: [200, 100],
        silent: false,
        onClick: function () {
            window.focus();
        },
        onClose: function () {
            this.close();
        },
        onError: function () {
            console.log('onError in notification!');
        },
        onShow: function () {
        },
    });
}
function renderMessage(name, time, message) {
    if (!CheckWinFocus())
        GetNotification(name, time, message);

    var nameSpan = document.createElement('span');
    nameSpan.className = 'name';
    nameSpan.textContent = name;

    var timeSpan = document.createElement('span');
    timeSpan.className = 'time';
    var friendlyTime = moment(time).format('H:mm');
    timeSpan.textContent = friendlyTime;

    var headerDiv = document.createElement('div');
    headerDiv.appendChild(nameSpan);
    headerDiv.appendChild(timeSpan);

    var messageDiv = document.createElement('div');
    messageDiv.className = 'message';
    messageDiv.innerHTML = message;

    var newItem = document.createElement('li');
    newItem.appendChild(headerDiv);
    newItem.appendChild(messageDiv);

    var chatHistoryEl = document.getElementById('chatHistory');
    chatHistoryEl.appendChild(newItem);
    chatHistoryEl.scrollTop = chatHistoryEl.scrollHeight - chatHistoryEl.clientHeight;
}

document.addEventListener('DOMContentLoaded', ready);