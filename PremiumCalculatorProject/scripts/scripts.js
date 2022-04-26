//SELECTORS
const getPremiumButton = document.querySelector(("[id='getPremium']"));
const dateofbirthDatepicker = document.querySelector(("[id='dateofbirth']"));
const divToAppend = document.getElementById("getDataFromWS");
const divToAppendMessage = document.getElementById("messageBox");
const selectState = document.getElementById("states");
const selectPlan = document.getElementById("plans");

//LISTENERS
getPremiumButton.addEventListener('click', getPremium);
dateofbirthDatepicker.addEventListener('change', calculateAge);
window.onload = AddOptionState();
window.onload = AddOptionPlan();
window.onload = GetRulesCount();

//FUNCTIONS
function AddOptionState() {
    //CLEAR OPTIONS
    $.ajax({
        type: 'POST',
        url: 'PremiumWebService.asmx/GetStates',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            var stateList = JSON.parse(response.d);
            var fullOptions = "<option hidden value = \"\" > --Select option--</option>";
            $.each(stateList, function (index, state) {
                var option = "<option value=\"" + state.state + "\">" + state.stateName + "</option>";
                fullOptions = fullOptions.concat(' ',option);
            });
            selectState.innerHTML = selectState.innerHTML + fullOptions;
        }
    });
}

function AddOptionPlan() {
    //CLEAR OPTIONS
    $.ajax({
        type: 'POST',
        url: 'PremiumWebService.asmx/GetPlans',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            var planList = JSON.parse(response.d);
            var fullOptions = "<option hidden value = \"\" > --Select option--</option>";
            $.each(planList, function (index, plan) {
                var option = "<option value=\"" + plan.plan + "\">" + plan.plan + "</option>";
                fullOptions = fullOptions.concat(' ', option);
            });
            selectPlan.innerHTML = selectPlan.innerHTML + fullOptions;
        }
    });
}

function GetRulesCount() {
    //CLEAR OPTIONS
    divToAppendMessage.innerHTML = "";

    $.ajax({
        type: 'POST',
        url: 'PremiumWebService.asmx/GetPremiumRules',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            var premiumCount = JSON.parse(response.d);

            const divButton = document.createElement("div");
            divButton.className = "header-element";
            const inputButton = document.createElement("input");
            inputButton.className = "linkButton";
            inputButton.type = "submit";
            inputButton.onclick = redirectToList;
            inputButton.value = "Rules: " + premiumCount.length + ".   Add more!";
            divButton.appendChild(inputButton);

            divToAppendMessage.appendChild(divButton);
        }
    });
}

function redirectToList() {
    window.location.href = 'PremiumList.html';
}

function calculateAge() {
    var dateofbirth = document.getElementById('dateofbirth');
    var dob = new Date(dateofbirth.value);

    if (isNaN(dob))
        dateofbirth.classList.add('warning');
    else {
        dateofbirth.classList.remove('warning');

        var today = new Date();
        var age = today.getFullYear() - dob.getFullYear();

        var monthDiff = today.getMonth() - dob.getMonth();
        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate()))
            age--;

        document.getElementById('age').value = age;
    }
}

function getPremium(event) {
    event.preventDefault();
    var emptyFields = 0;

    //CLEAR DIVS
    divToAppend.innerHTML = "";

    // VERIFY DATE OF BIRTH
    var dateofbirth = document.getElementById('dateofbirth');
    var age = document.getElementById('age');
    var dob = new Date(dateofbirth.value);
    if (isNaN(dob)) {
        dateofbirth.classList.add('warning');
        emptyFields++;
    }
    else {
        dateofbirth.classList.remove('warning');
        age.classList.remove('warning');
    }

    // VERIFY STATES
    var states = document.getElementById('states');
    var state = states.options[states.selectedIndex];
    if (state.value == '') {
        states.classList.add('warning');
        emptyFields++;
    }
    else
        states.classList.remove('warning');

    // VERIFY PLAN
    var plans = document.getElementById('plans');
    var plan = plans.options[plans.selectedIndex];
    if (plan.value == '') {
        plans.classList.add('warning');
        emptyFields++;
    }
    else
        plans.classList.remove('warning');

    // VERIFY PERIOD
    var periods = document.getElementById('periods');
    var period = periods.options[periods.selectedIndex];
    if (period.value == '') {
        periods.classList.add('warning');
        emptyFields++;
    }
    else
        periods.classList.remove('warning');

    //IF FIELDS EMPTY, RETURN
    if (emptyFields > 0)
        return;

    //CALL WEB SERVICE
    var parameters = "{dateofbirth:'" + dateofbirth.value + "',state:'" + state.value + "',age:'" + age.value + "',plan:'" + plan.value + "'}";

    $.ajax({
        type: 'POST',
        data: parameters,
        url: 'PremiumWebService.asmx/PremiumCalculator',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            var premiumList = JSON.parse(response.d);
            if (premiumList.status === undefined) {

                $.each(premiumList, function (index, premium) {

                    //CALCULATIONS
                    var carrier = premium.carrier;
                    var premiumPlan = parseFloat(premium.premium);
                    var monthly = premiumPlan / parseInt(period.value);
                    var annual = monthly * 12;

                    //CREATE NEW DIV WITH DATA RETRIEVED
                    const divRightAmount = document.createElement("div");
                    divRightAmount.className = "body-column-right-amount";

                    //CARRIER
                    const divCarrier = document.createElement("div");
                    divCarrier.className = "body-element center";
                    const inputCarrier = document.createElement("input");
                    inputCarrier.className = "body-box bigger";
                    inputCarrier.type = "Text";
                    inputCarrier.readOnly = true;
                    inputCarrier.value = carrier;
                    divCarrier.appendChild(inputCarrier);

                    //PREMIUM
                    const divPremium = document.createElement("div");
                    divPremium.className = "body-element center";
                    const inputPremium = document.createElement("input");
                    inputPremium.className = "body-box bigger";
                    inputPremium.type = "Text";
                    inputPremium.readOnly = true;
                    inputPremium.value = premiumPlan.toFixed(2);
                    divPremium.appendChild(inputPremium);

                    //ANNUAL
                    const divAnnual = document.createElement("div");
                    divAnnual.className = "body-element center";
                    const inputAnnual = document.createElement("input");
                    inputAnnual.className = "body-box bigger";
                    inputAnnual.type = "Text";
                    inputAnnual.readOnly = true;
                    inputAnnual.value = annual.toFixed(2);
                    divAnnual.appendChild(inputAnnual);

                    //MONTHLY
                    const divMonthly = document.createElement("div");
                    divMonthly.className = "body-element center";
                    const inputMonthly = document.createElement("input");
                    inputMonthly.className = "body-box bigger";
                    inputMonthly.type = "Text";
                    inputMonthly.readOnly = true;
                    inputMonthly.value = monthly.toFixed(2);
                    divMonthly.appendChild(inputMonthly);

                    //APPEND NEW DIV
                    divRightAmount.appendChild(divCarrier);
                    divRightAmount.appendChild(divPremium);
                    divRightAmount.appendChild(divAnnual);
                    divRightAmount.appendChild(divMonthly);
                    divToAppend.appendChild(divRightAmount);
                });
            } else {
                //ERROR MESSAGE FROM WS
                const divMessage = document.createElement("div");
                divMessage.className = "body-element center";
                const inputMessage = document.createElement("Label");
                inputMessage.className = "body-label labelwarning";
                inputMessage.innerHTML = premiumList.message;
                divMessage.appendChild(inputMessage);

                divToAppend.appendChild(divMessage);
            }
        }
    });
}