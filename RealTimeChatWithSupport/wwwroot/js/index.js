var chatterName = 'Visitor';
var myTimeOut;
var Questions;
var formId = '';
var dialogEl = document.getElementById('chatDialog');
var questionAnswers = [];
// Initialize the SignalR client
var connection = new signalR.HubConnectionBuilder()
    .withUrl('/chatHub')
    .build();

connection.on('ReceiveMessage', renderMessage);
connection.on('PassRoomId', PassId);
connection.on('RunTimeOut', runSetTimeOut);
connection.on('QuestionList', QuestionsValues)
connection.on('GenerateFormId', SetFormId)

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
function runSetTimeOut() {
    clearTimeout(myTimeOut);
    myTimeOut = window.setTimeout(function () {
        connection.invoke('GetQuestions');
    }, 20000);

}
function QuestionsValues(values, pageIndex = 0) {
    $("#bottomPanel").hide();
    Questions = values.result;
    SetFormValues(Questions[pageIndex], pageIndex);

}
function SetFormId(frmId) {
    formId = frmId;

}
function SetFormValues(question, pageIndex) {
    var myForm = '';

    if (parseInt(pageIndex) > Questions.length - 1) {
        myForm = '<p>Thank you for store vote !</p>'; // Pressed done btn
        AssignedForm(myForm);
        return;
    }

    myForm = `<div id="Survey"  style="border-style:groove;border-radius: 5px;padding: 5px;">
    <input type="hidden" id="QuestionId" value="`+ question.id + `" />
    <p>`+ question.questionTitle + `</p>`;
    if (questionAnswers.length > 0) {
        var selectedAnswer = questionAnswers.filter(function (index) {
            return index.id == question.id;
        })
        var check = "checked";
        console.log(selectedAnswer);
        if (selectedAnswer.length > 0) {
            if (selectedAnswer[0].answer == 1) {
                myForm += `<input type="radio" name="option" ` + check + ` value='1' /> ` + question.firstOption + `
                    <br />`;
            }
            else {
                myForm += `<input type="radio" name="option" value='1' /> ` + question.firstOption + `
                    <br />`;
            }

            if (selectedAnswer[0].answer == 2) {
                myForm += `<input type="radio" name="option" ` + check + ` value='2' /> ` + question.secondOption + `
                    <br />`;
            } else {
                myForm += `<input type="radio" name="option" value='2' /> ` + question.secondOption + `
                    <br />`;
            }

            if (selectedAnswer[0].answer == 3) {
                myForm += `<input type="radio" name="option" ` + check + ` value='3' /> ` + question.thirdOption + `
                    <br />`;
            } else {
                myForm += `<input type="radio" name="option" value='3' /> ` + question.thirdOption + `
                    <br />`;
            }

            if (selectedAnswer[0].answer == 4) {
                myForm += `<input type="radio" name="option" ` + check + ` value='4' /> ` + question.fourthOption + `
                    <br />`;
            }
            else {
                myForm += `<input type="radio" name="option"  value='4' /> ` + question.fourthOption + `
                    <br />`;
            }
        }
        else {
            myForm += `<input type="radio" name="option"  value='1' /> ` + question.firstOption + `
                        <br />
                        <input type="radio" name="option"  value='2' /> ` + question.secondOption + `
                        <br />
                        <input type="radio" name="option"  value='3' /> ` + question.thirdOption + `
                        <br />
                        <input type="radio" name="option"  value='4' /> ` + question.fourthOption + `
                        <br />`;
        }

    }
    else {
        myForm += `<input type="radio" name="option" value='1' /> ` + question.firstOption + `
                    <br />
                    <input type="radio" name="option" value='2' /> ` + question.secondOption + `
                    <br />
                    <input type="radio" name="option" value='3' /> ` + question.thirdOption + `
                    <br />
                    <input type="radio" name="option" value='4' /> ` + question.fourthOption + `
                    <br />`;
    }

    if (parseInt(pageIndex) <= 0) {
        myForm += `<button onclick="NextFunc(\`` + "1" + `\`)">Next</button>`
    }
    else if (parseInt(pageIndex) == Questions.length - 1) {
        myForm += `<button onclick="NextFunc(` + (parseInt(pageIndex) + 1) + `)">Done</button>
        <button onclick="PreviousFunc(` + (parseInt(pageIndex) - 1) + `)">Previous</button>`;

    }
    else {
        myForm += `<button onclick="NextFunc(` + (parseInt(pageIndex) + 1) + `)">Next</button>
        <button onclick="PreviousFunc(` + (parseInt(pageIndex) - 1) + `)">Previous</button>`;
    }
    myForm += `</div >`;


    AssignedForm(myForm);


}

function AssignedForm(myForm) {
    var messageDiv = document.createElement('div');
    messageDiv.className = 'message';
    messageDiv.innerHTML = myForm;

    var newItem = document.createElement('li');
    newItem.appendChild(messageDiv);
    $("#Survey").remove();
    var chatHistoryEl = document.getElementById('chatHistory');
    chatHistoryEl.appendChild(newItem);
    chatHistoryEl.scrollTop = chatHistoryEl.scrollHeight - chatHistoryEl.clientHeight;
}
function NextFunc(pageIndex) {

    var optionChecked = $('input[name="option"]:checked').val();


    if (optionChecked == 'undefined' || optionChecked == null) {
        alert('please select your option!'); return;
    }
    $.ajax({
        "url": "/Home/SetVote",
        "type": "POST",
        "datatype": "json",
        "data": {
            id: $("#QuestionId").val(), answer: optionChecked, formid: formId },
        success: function (res) {
            if (res.success) {
                questionAnswers = jQuery.grep(questionAnswers, function (value) {
                    return value.id != $("#QuestionId").val();// pop duplicate value from array
                });
                var answers = {
                    id: $("#QuestionId").val(),
                    answer: parseInt(optionChecked)
                }

                questionAnswers.push(answers);
                SetFormValues(Questions[pageIndex], pageIndex);
            }
        },
        error: function (msg) {
            alert('error in submit vote!');
        }

    });
}
function PreviousFunc(pageIndex) {
    SetFormValues(Questions[pageIndex], pageIndex);
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
    clearTimeout(myTimeOut);
    myTimeOut = window.setTimeout(function () {
        connection.invoke('GetQuestions');
    }, 20000);
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
    Push.create(`Message from` + name, {
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