var activeRoomId = '';
var agentConnection = new signalR.HubConnectionBuilder()
    .withUrl('/agentHub')
    .build();

agentConnection.on('ActiveRooms', loadRooms);

agentConnection.onclose(function () {
    handleDisconnected(startAgentConnection);
});

function startAgentConnection() {
    agentConnection
        .start()
        .catch(function (err) {
            console.error(err);
        });

}
function cancelChat() {
    var roomId = activeRoomId;
    chatConnection.invoke('CancelChat', roomId);
}
var chatConnection = new signalR.HubConnectionBuilder()
    .withUrl('/chatHub')
    .build();

chatConnection.onclose(function () {
    handleDisconnected(startChatConnection);
});

chatConnection.on('ReceiveMessage', addMessage);
agentConnection.on('ReceiveMessages', addMessages);
agentConnection.on('PassRoomId', PassId);

agentConnection.on('RunTimer', runTimerFunc);

function runTimerFunc() {
    chatConnection.invoke('InitSurveyForm', activeRoomId);
}


function startChatConnection() {
    chatConnection
        .start()
        .catch(function (err) {
            console.error(err);
        });
}
function PassId(roomId) {
    var chatFormEl = document.getElementById('fileForm');
    chatFormEl[1].value = roomId;
}

function handleDisconnected(retryFunc) {
    timeOut = setTimeout(retryFunc, 5000);
    clearTimeout()

}

function sendMessage(text) {
    if (text && text.length) {
        agentConnection.invoke('SendAgentMessage', activeRoomId, text);
    }
}

function ready() {
    startAgentConnection();
    startChatConnection();

    var chatFormEl = document.getElementById('chatForm');
    chatFormEl.addEventListener('submit', function (e) {
        e.preventDefault();

        var text = e.target[0].value;
        e.target[0].value = '';
        sendMessage(text);
    });
}

function switchActiveRoomTo(id) {
    if (id === activeRoomId) return;

    if (activeRoomId) {
        chatConnection.invoke('LeaveRoom', activeRoomId);
    }

    activeRoomId = id;
    removeAllChildren(roomHistoryEl);

    if (!id) return;

    chatConnection.invoke('JoinRoom', activeRoomId);
    agentConnection.invoke('LoadHistory', activeRoomId);

}


var roomListEl = document.getElementById('roomList');
var roomHistoryEl = document.getElementById('chatHistory');

//when send message with select Send btn

roomListEl.addEventListener('click', function (e) {
    //roomHistoryEl.style.display = 'block';
    //setActiveRoomButton(e.target);

    var roomId = e.target.getAttribute('data-id');
    alert(roomId);
    switchActiveRoomTo(roomId);
    agentConnection.invoke('GetRoomID', activeRoomId);
    agentConnection.invoke('GetRoomIdForUpFile');

});

agentConnection.on("GetRoomIdForFilterRooms", (roomId) => {
    $('#roomList a[data-id="' + roomId + '"]').remove();
});

agentConnection.on("SetNewRoom", (room) => {
    var roomInfo = room;
    if (!roomInfo.name) return;
    var roomButton = createRoomButton(roomInfo);
    $("#roomList").append(roomButton);
});
// when support select room
function setActiveRoomButton(el) {
    var allButtons = roomListEl.querySelectorAll('a.list-group-item');

    allButtons.forEach(function (btn) {
        btn.classList.remove('active');
    });

    el.classList.add('active');
}
// load all rooms for support
function loadRooms(rooms) {
    if (!rooms) return;

    switchActiveRoomTo(null);
    removeAllChildren(roomListEl);

    rooms.forEach(function (item, index) {
        var roomInfo = rooms[index];
        if (!roomInfo.name) return;
        var roomButton = createRoomButton(roomInfo);
        $("#roomList").append(roomButton);
    });
    

}



//Create room with a tag
function createRoomButton(roomInfo) {
    var header = roomInfo.name.charAt(0);

    var room = `<li>
                    <a href="#" data-id=`+ roomInfo.id + `>
                        <div class="media">
                            <div class="chat-user-img online align-self-center mr-3">
                                <div class="avatar-xs">
                                    <span class="avatar-title rounded-circle bg-soft-primary text-primary">
                                        ` + header.toUpperCase() + `
                                    </span>
                                </div>
                                <span class="user-status"></span>
                            </div>
                            <div class="media-body overflow-hidden">
                                <h5 class="text-truncate font-size-15 mb-1">`+ roomInfo.name + `</h5>
                                <p class="chat-user-message text-truncate mb-0">Hey! there I'm available</p>
                            </div>
                            <div class="font-size-11">05 min</div>
                        </div>
                    </a>
                </li>`;
    
    return room;
}
// Send message
function addMessages(messages) {
    if (!messages) return;

    messages.forEach(function (m) {
        addMessage(m.senderName, m.sentAt, m.text);
    });
}
// if chat window is not focused call GetNotification function
function CheckWinFocus() {
    if (document.hasFocus()) return true;
    else return false;

}
// Call notification function
function GetNotification(name, time, message) {
    Push.create(`Message from ` + name, {
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
// show user messages in form
function addMessage(name, time, message) {
    if (!CheckWinFocus())
        GetNotification(name, time, message);
    console.log(name + ' ' + time + ' ' + message)
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

    roomHistoryEl.appendChild(newItem);
    roomHistoryEl.scrollTop = roomHistoryEl.scrollHeight - roomHistoryEl.clientHeight;
}

function removeAllChildren(node) {
    if (!node) return;

    while (node.lastChild) {
        node.removeChild(node.lastChild);
    }
}

document.addEventListener('DOMContentLoaded', ready);