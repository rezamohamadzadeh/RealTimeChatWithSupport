var activeRoomId = '';
var userName = '';
var agentConnection = new signalR.HubConnectionBuilder()
    .withUrl('/agentHub')
    .build();

agentConnection.on('ActiveRooms', loadRooms);
agentConnection.on('GetUserName', setUserName);

agentConnection.onclose(function () {
    handleDisconnected(startAgentConnection);
});

function setUserName(user) {
    userName = user;
}
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
    $("#RoomId").val(roomId)
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

    $("#chatForm").submit(function (e) {
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
    var userAvatarName = $('#' + activeRoomId).val();
    var header = userAvatarName.charAt(0);

    $("#userAvatarOnChatHeader").html(`<div class="avatar-xs">
                                            <span class="avatar-title rounded-circle bg-soft-primary text-primary">
                                                `+ header + `
                                            </span>
                                        </div>`);
    $("#userAvatarOnChatHeader").html(`<h5 class="font-size-16 mb-0 text-truncate"><a href="#" class="text-reset user-profile-show">` + userAvatarName + `</a> <i class="ri-record-circle-fill font-size-10 text-success d-inline-block ml-1"></i></h5>`);


}


var roomListEl = document.getElementById('roomList');
var roomHistoryEl = document.getElementById('chatHistory');


$("#roomList").click(function (e) {
    var roomId = e.target.getAttribute('data-id');

    $("#roomList li a").click(function (e) {
        $("#roomList li").removeClass("active");
        $(this).closest('li').addClass('active');
    });

    $("#chatDiv").show();
    $("#chatForm").show();
    $("#chatHistory").show();

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
    var newRoomButton = createRoomButton(roomInfo);

    var roomButton = document.getElementById('roomList').innerHTML;
    var room = newRoomButton + roomButton;
    $("#roomList").html(room);
});

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

function openfile(a) {
    $(a).trigger('click');
}
function UpFile() {
    // Checking whether FormData is available in browser  
    if (window.FormData !== undefined) {

        var fileUpload = $("#file-input").get(0);
        var files = fileUpload.files;

        // Create FormData object  
        var fileData = new FormData();
        fileData.append("RoomId", $("#RoomId").val());
        // Looping over all files and add it to FormData object  
        for (var i = 0; i < files.length; i++) {
            fileData.append(files[i].name, files[i]);
        }

        $.ajax({
            url: '/Agent/GetFile',
            type: "POST",
            contentType: false, // Not to set any content header  
            processData: false, // Not to process data  
            data: fileData,
            success: function (result) {
                console.log(result);
            },
            error: function (err) {
                console.log(err.statusText);
            }
        });
    } else {
        alert("FormData is not supported.");
    }
}

//Create room with a tag
function createRoomButton(roomInfo) {
    var header = roomInfo.name.charAt(0);

    var id = roomInfo.id;
    var room = `<li>
                    <a href="#" data-id=`+ roomInfo.id + `>
                        <input type="hidden" id=`+ id +` value=`+ roomInfo.name +` />
                        <div class="media" data-id=`+ roomInfo.id + ` class="list-group-item">
                            <div class="chat-user-img online align-self-center mr-3" data-id=`+ roomInfo.id + ` >
                                <div class="avatar-xs" data-id=`+ roomInfo.id + `>
                                    <span class="avatar-title rounded-circle bg-soft-primary text-primary" data-id=`+ roomInfo.id + `>
                                        ` + header.toUpperCase() + `
                                    </span>
                                </div>
                            </div>
                            <div class="media-body overflow-hidden" data-id=`+ roomInfo.id + `>
                                <h5 class="text-truncate font-size-15 mb-1" data-id=`+ roomInfo.id + `>` + roomInfo.name + `</h5>
                                <p class="chat-user-message text-truncate mb-0" data-id=`+ roomInfo.id + `>Hey! there I'm available</p>
                            </div>
                        </div>
                    </a>
                </li>`;

    return room;
}
// Send message
function addMessages(messages) {
    if (!messages) return;

    messages.forEach(function (m) {
        addMessage(m.senderName, m.dateTime, m.text);
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

    var header = name.charAt(0);
    var chatMessage = '';
    if (userName !== name) {
        chatMessage = `<li>
                        <div class="conversation-list">
                            <div class="avatar-xs">
                                <span class="avatar-title rounded-circle bg-soft-primary text-primary">
                                    ` + header.toUpperCase() + `
                                </span>
                            </div>
                        
                            <div class="user-chat-content">
                                <div class="ctext-wrap">
                                    <div class="ctext-wrap-content">
                                        <p class="mb-0">
                                            `+ message + `
                                        </p>
                                        <p class="chat-time mb-0"><i class="ri-time-line align-middle"></i> <span class="align-middle">`+ time + `</span></p>
                                    </div>
                                    
                                </div>
                                <div class="conversation-name">`+ name + `</div>
                            </div>
                        </div>
                </li>`;
    }
    else {
        chatMessage = `<li class="right">
                            <div class="conversation-list">
                                <div class="chat-avatar">
                                    <span class="avatar-title rounded-circle bg-soft-primary text-primary">
                                    ` + header.toUpperCase() + `
                                </span>
                                </div>
    
                                <div class="user-chat-content">
                                    <div class="ctext-wrap">
                                        <div class="ctext-wrap-content">
                                            <p class="mb-0">
                                                `+ message + `
                                            </p>
                                            <p class="chat-time mb-0"><i class="ri-time-line align-middle"></i> <span class="align-middle">`+ time + `</span></p>
                                        </div>
                                                 
                                    </div>
                                    <div class="conversation-name">`+ name + `</div>
                                </div>
                            </div>
                        </li>`;
    }



    $("#chatHistory").append(chatMessage);


    roomHistoryEl.scrollTop = roomHistoryEl.scrollHeight - roomHistoryEl.clientHeight;

}
function removeAllChildren(node) {
    if (!node) return;

    while (node.lastChild) {
        node.removeChild(node.lastChild);
    }
}

document.addEventListener('DOMContentLoaded', ready);