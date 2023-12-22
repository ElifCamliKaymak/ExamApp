let questionAnswerChoices = document.getElementsByClassName("choices");
let studentAnswers = [];
let studentQuestonId = document.getElementById("StudentQuestionId").value;

let timeGiven = document.getElementById("TimeGiven").value.split(':').reduce(function (seconds, v) {
    return + v + seconds * 60;
}, 0);

let timeStarted = new Date(document.getElementById("TimeStarted").value);

document.getElementById("next-question-button").addEventListener("click", sendAnswer);

[...questionAnswerChoices].forEach(function (item) {
    studentAnswers.push({ IsSelected: false, StudentQuestionId: studentQuestonId, QuestionAnswerId: item.value });
});

document.getElementById("studentAnswers").value = JSON.stringify(studentAnswers);

function updateStudentAnswers() {
    studentAnswers.forEach((item, index) => {
        item.IsSelected = document.getElementById(`answer-${index + 1}`).checked;
    });
    document.getElementById("studentAnswers").value = JSON.stringify(studentAnswers);
}

document.getElementById("transitionCounterMain").style.display = "none";

let questionTimer = setInterval(function () {

    let questionCounterText = "Kalan Süre: ";

    let counter = timeGiven - Math.floor((Date.now() - timeStarted) / 1000);
    if (counter > 5) {
        document.getElementById("questionCounter").innerHTML = counter;
        document.getElementById("questionCounterText").innerHTML = questionCounterText;
    }
    else if (counter > 0) {
        document.getElementById("questionCounter").style.color = "#e6353b";
        document.getElementById("questionCounter").innerHTML = counter;
        document.getElementById("questionCounterText").innerHTML = questionCounterText;
    }
    else {
        document.getElementById("questionCounter").innerHTML = 0;
        document.getElementById("questionCounterText").innerHTML = questionCounterText;
        sendAnswer();
        document.getElementById("questionCounterMain").style.display = "none";
        document.getElementById("transitionCounterMain").style.display = "block";
        transitionCountdown();
    }
}, 1000);

function sendAnswer() {
    clearInterval(questionTimer);
    [...questionAnswerChoices].forEach(function (item) {
        item.setAttribute("disabled", "true")
    });
}

function transitionCountdown() {
    let transitionCounterTimeAmount = 5;
    let transitionCounterText = "";
    if (questionCount !== currentQuestionOrder) {
        transitionCounterText = " Saniye Sonra Diğer Soruya Geçilecektir...";
    } else {
        transitionCounterText = " Saniye Sonra Sınavınız Sona Erecektir...";
    }

    let transitionTimer = setInterval(function () {
        if (transitionCounterTimeAmount > 0) {
            // Henüz 0'a ulaşmadıysa, kalan süreyi göster.
            document.getElementById("transitionCounter").style.color = "#e6353b";
            document.getElementById("transitionCounterText").innerHTML = transitionCounterText;
            document.getElementById("transitionCounter").innerHTML = transitionCounterTimeAmount;
            transitionCounterTimeAmount--;
        } else {
            // Sayaç 0'a ulaştıysa, işlem durdurulmalı ve sonraki soruya geçilmeli.
            clearInterval(transitionTimer);
            document.getElementById("next-question-button").click();
        }
    }, 1000);
}