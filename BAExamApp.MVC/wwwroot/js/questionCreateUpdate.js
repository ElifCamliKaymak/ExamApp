let subjects = [];
let subtopics = [];
let questionAnswerChoices = [];
let questionDifficulties = [];

document.getElementById("questionAnswerChoices").value = JSON.stringify(questionAnswerChoices);

if ($("#ProductId").val()) {
    onProductChange().then(() => {
        document.getElementById("SubjectId").value = document.getElementById("subjectIdFromReload").value;
        onSubjectChange().then(() => {
            document.getElementById("SubtopicId").value = document.getElementById("subtopicIdFromReload").value;
        });
    });
}

if ($("#QuestionType").val()) {
    questionAnswerChoices = JSON.parse(document.getElementById("questionAnswersListFromReload").value);
    createQuestionAnswersHtml($("#QuestionType").val());
}

if ($("#QuestionDifficultyId").val()) {
    onQuestionDifficultyChange();
}

//Ajax functions for getting selectList
async function getSubjects(selectedProductId) {
    return $.ajax({
        url: '/Trainer/Question/GetSubjectsByProductId',
        data: { productId: selectedProductId },
    });
}
async function getSubtopics(selectedSubjectId) {
    return $.ajax({
        url: '/Trainer/Question/GetSubtopicsBySubjectId',
        data: { subjectId: selectedSubjectId },
    });
}
async function getTime(selectedQuestionDifficultyId) {
    return $.ajax({
        url: '/Trainer/Question/GetQuestionTimeByDifficultyId',
        data: { difficultyId: selectedQuestionDifficultyId },
    });
}

async function getQuestionDifficulties(selectedQuestionTypeId) {
    return $.ajax({
        url: '/Trainer/Question/GetQuestionDifficulties',
        data: { questionTypeId: selectedQuestionTypeId },
    });
}


//Product type change event.
async function onProductChange() {
    subjects = subjects ? await getSubjects($("#ProductId").val()) : subjects;
    populateSelectList("SubjectId", subjects);
    subtopics = [];
    populateSelectList("SubtopicId", subtopics);
};

//Product type change event.
async function onSubjectChange() {
    subtopics = subtopics ? await getSubtopics($("#SubjectId").val()) : subtopics;
    populateSelectList("SubtopicId", subtopics);
};

//Question type change event.
async function onQuestionTypeChange() {
    questionDifficulties = questionDifficulties ? await getQuestionDifficulties($("#QuestionType").val()) : questionDifficulties;
    populateSelectList("QuestionDifficultyId", questionDifficulties);
    questionDifficulties = [];

    let questionType = document.getElementById("QuestionType").value;

    if (questionType == 3) {
        questionAnswerChoices = [{ Answer: "True", IsRightAnswer: false }, { Answer: "False", IsRightAnswer: false }];
    }
    else {
        questionAnswerChoices = [{ Answer: "", IsRightAnswer: false }, { Answer: "", IsRightAnswer: false }];
    }

    createQuestionAnswersHtml(questionType);
};


//Question difficulty change event.
async function onQuestionDifficultyChange() {
    document.getElementById("TimeGiven").value = await getTime($("#QuestionDifficultyId").val());
};

async function createQuestionAnswersHtml(questionType) {
    switch (questionType) {
        case '1':
            {
                document.getElementById("questionAnswersDiv").innerHTML = await getAnswerChoicesHtml("checkbox");
                document.getElementById("questionAnswerChoices").value = JSON.stringify(questionAnswerChoices);
                document.getElementById("questionAnswersDiv").removeAttribute("hidden");
                break;
            }
        case '2':
            {
                document.getElementById("questionAnswersDiv").innerHTML = await getAnswerChoicesHtml("radio");
                document.getElementById("questionAnswerChoices").value = JSON.stringify(questionAnswerChoices);
                document.getElementById("questionAnswersDiv").removeAttribute("hidden");
                break;

            }
        case '3':
            document.getElementById("questionAnswersDiv").innerHTML =
                `<label class="col-sm-12 col-form-label col-form-label-lg" for="QuestionAnswers">Yanıtlar</label>
                <div class="form-check form-check-inline" id="choices">
                    <div class="form-check form-check-inline">
                        <input class="form-check-input" type="radio" name="answerOptions" id="choice0"  onChange="updateCheckedAnswers()">
                        <label class="form-check-label" for="true">True</label>
                    </div>
                    <div class="form-check form-check-inline">
                        <input class="form-check-input" type="radio" name="answerOptions" id="choice1"  onChange="updateCheckedAnswers()">
                        <label class="form-check-label" for="false">False</label>
                    </div>
                </div>`;
            document.getElementById("questionAnswerChoices").value = JSON.stringify(questionAnswerChoices);
            document.getElementById("questionAnswersDiv").removeAttribute("hidden");
            break;
        case '4':
            document.getElementById("questionAnswersDiv").innerHTML = await getClassicQuestionAnswer();
            document.getElementById("questionAnswersDiv").removeAttribute("hidden");
            break;
        default:
            document.getElementById("questionAnswersDiv").setAttribute("hidden", true);
            break;
    }
}

async function getClassicQuestionAnswer() {
    return `<label class="col-sm-12 col-form-label col-form-label-lg" for="QuestionAnswers">Yanıt</label>
                 <textarea name="QuestionAnswers" id="classicQuestionAnswer" rows="4" class="form-control form-control-lg form-control-solid for="QuestionAnswers" oninput="updateQuestionAnswerChoices(this.value)"></textarea>`;
}

function updateQuestionAnswerChoices(value) {
    questionAnswerChoices = [];
    questionAnswerChoices.push({ Answer: value, IsRightAnswer: true });
    document.getElementById("questionAnswerChoices").value = JSON.stringify(questionAnswerChoices);
}

async function getAnswerChoicesHtml(choiceType) {
    return `<label class="col-sm-12 col-form-label col-form-label-lg" for="QuestionAnswers">Yanıtlar</label>`
        .concat(questionAnswerChoices.map((item, index) =>
            `<div class="form-check" id="choices">
                <div class="row g-3">
                    <div class="form-check col-sm-1">
                        <input class="form-check-input" type="${choiceType}" name="answerOptions" id="choice${index}" ${item.IsRightAnswer ? `checked` : ``} onChange="updateCheckedAnswers()">
                    </div>
                    <div class="col-sm-9">
                        <input type="text" class="form-control form-control-solid" id="answerText${index}" placeholder = "Yeni Seçenek"; value = "${item.Answer}";  onChange="updateAnswerText(${index})"/>
                    </div>
                    <div class="col-sm-2">
                        <button class="btn btn-danger btn-sm" type="button" onclick="removeChoice(${index},'${choiceType}')"> X </button>
                    </div>
                </div>
            </div>`).join(""),
            `<button class="btn btn-primary btn-sm" type="button" onclick="addNewChoice('${choiceType}')"> Yeni Seçenek Ekle </button>`);
}

function updateAnswerText(index) {
    questionAnswerChoices[index].Answer = document.getElementById(`answerText${index}`).value;
    document.getElementById("questionAnswerChoices").value = JSON.stringify(questionAnswerChoices);
}

function updateCheckedAnswers() {
    questionAnswerChoices.forEach((item, index) => {
        item.IsRightAnswer = document.getElementById(`choice${index}`).checked;
    });
    document.getElementById("questionAnswerChoices").value = JSON.stringify(questionAnswerChoices);
}
async function addNewChoice(choiceType) {
    questionAnswerChoices.push({ Answer: "", IsRightAnswer: false });
    document.getElementById("questionAnswerChoices").value = JSON.stringify(questionAnswerChoices);
    document.getElementById("questionAnswersDiv").innerHTML = await getAnswerChoicesHtml(choiceType);

    questionAnswerChoices.forEach((item, index) => {
        document.getElementById(`answerText${index}`).placeholder = "Yeni Seçenek";
    });
}

async function removeChoice(index, choiceType) {
    questionAnswerChoices.splice(index, 1);
    document.getElementById("questionAnswerChoices").value = JSON.stringify(questionAnswerChoices);
    document.getElementById("questionAnswersDiv").innerHTML = await getAnswerChoicesHtml(choiceType);
}

//Function for populating select lists
async function populateSelectList(selectListId, data) {
    let selectListOptions = data.map((item, index) => `<option value="${item.value}">${item.text}</option>`);
    let selectList = `<option value="" disabled="" selected="">--- Seçiniz ---</option>`.concat(selectListOptions);
    document.getElementById(selectListId).innerHTML = selectList;
}


$(document).ready(function () {
});

$("#productıd").change(function () {
});

$("button[type='submit']").click(function (event) {
    event.preventDefault();

    if (validateCheckBoxes()) {
        $("form").submit();
    }
});

$(document).ready(function () {
    validateCheckBoxes();
});

$("#ProductId").change(function () {
    validateCheckBoxes();
});

src = "~/lib/limonte-sweetalert2/sweetalert2.all.js";

function validateCheckBoxes() {
    let questionType = $("#QuestionType").val();
    if (questionType === "1" || questionType === "2" || questionType === "3" || questionType === "4") {
        if (questionType !== "4" && $("input[name='answerOptions']:checked").length === 0 || questionType === "4" && $("#classicQuestionAnswer").val().length === 0) {
            Swal.fire({
                title: 'Eksik Veri Girildi',
                text: 'Sorunun Doğru Cevabını veya Soru Bilgilerinizin Tamamını Girmediniz!',
                icon: 'error'
            });
            return false;
        }
        else {
            return true;
        }
    }
}