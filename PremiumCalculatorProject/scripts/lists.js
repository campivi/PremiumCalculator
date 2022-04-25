//SELECTORS
const getPremiumButton = document.querySelector("[id='addPremium']");
const divToAppend = document.getElementById("getRulesFromWS");

//LISTENERS
getPremiumButton.addEventListener('click', addPremium);
window.onload = getPremiumRules();

function addPremium(event) {
    event.preventDefault();
    var emptyFields = 0;

    //CLEAR DIVS
    divToAppend.innerHTML = "";

    var month = document.getElementById('newMonth');
    var carrier = document.getElementById('newCarrier');
    var state = document.getElementById('newState');
    var stateName = document.getElementById('newStateName');
    var plan = document.getElementById('newPlan');
    var minAge = document.getElementById('newMinAge');
    var maxAge = document.getElementById('newMaxAge');
    var premium = document.getElementById('newPremium');

    //CALL WEB SERVICE
    var parameters = "{carrier:'" + carrier.value + "',plan:'" + plan.value + "',state:'" + state.value + "',stateName:'" + stateName.value + "',month:'" + month.value + "',minAge:'" + minAge.value + "',maxAge:'" + maxAge.value + "',premium:'" + premium.value + "'}";

    $.ajax({
        type: 'POST',
        data: parameters,
        url: 'PremiumWebService.asmx/AddPremiumList',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            var premiumList = JSON.parse(response.d);
            if (premiumList.status != undefined) {
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

function deleteRule(e) {
    var item = e.target;
    var parent = item.parentElement
    var id = parent.getElementsByClassName("hidden")[0].value;
    console.log(id);

    //CALL WEB SERVICE
    var parameters = "{id:'" + id + "'}";

    $.ajax({
        type: 'POST',
        data: parameters,
        url: 'PremiumWebService.asmx/DeletePremiumRule',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            location.reload();            
        }
    });
}


function getPremiumRules(event) {
    //CLEAR DIVS
    divToAppend.innerHTML = "";

    //CALL WEB SERVICE
    $.ajax({
        type: 'POST',
        url: 'PremiumWebService.asmx/GetPremiumRules',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            var premiumList = JSON.parse(response.d);
            const ulElement = document.createElement("ul");
            ulElement.className = "body-list";

            $.each(premiumList, function (index, premium) {

                //CREATE NEW LIST WITH DATA RETRIEVED
                const liUnit = document.createElement("li");
                liUnit.className = "body-list-unit";

                //INNER DIVs

                //CARRIER
                const divCarrier = document.createElement("div");
                divCarrier.className = "body-element center col-8";
                const inputCarrier = document.createElement("input");
                inputCarrier.className = "body-box";
                inputCarrier.type = "Text";
                inputCarrier.readOnly = true;
                inputCarrier.value = premium.carrier;
                divCarrier.appendChild(inputCarrier);

                //PLAN
                const divPlan = document.createElement("div");
                divPlan.className = "body-element center col-8";
                const inputPlan = document.createElement("input");
                inputPlan.className = "body-box";
                inputPlan.type = "Text";
                inputPlan.readOnly = true;
                inputPlan.value = premium.plan;
                divPlan.appendChild(inputPlan);

                //STATE
                const divState = document.createElement("div");
                divState.className = "body-element center col-8";
                const inputState = document.createElement("input");
                inputState.className = "body-box";
                inputState.type = "Text";
                inputState.readOnly = true;
                inputState.value = premium.state + " " + premium.stateName;
                divState.appendChild(inputState);

                //MONTH
                const divMonth = document.createElement("div");
                divMonth.className = "body-element center col-8";
                const inputMonth = document.createElement("input");
                inputMonth.className = "body-box";
                inputMonth.type = "Text";
                inputMonth.readOnly = true;
                inputMonth.value = premium.month;
                divMonth.appendChild(inputMonth);

                //MIN AGE
                const divMinAge = document.createElement("div");
                divMinAge.className = "body-element center col-8";
                const inputMinAge = document.createElement("input");
                inputMinAge.className = "body-box";
                inputMinAge.type = "Text";
                inputMinAge.readOnly = true;
                inputMinAge.value = premium.minAge;
                divMinAge.appendChild(inputMinAge);

                //MAX AGE
                const divMaxAge = document.createElement("div");
                divMaxAge.className = "body-element center col-8";
                const inputMaxAge = document.createElement("input");
                inputMaxAge.className = "body-box";
                inputMaxAge.type = "Text";
                inputMaxAge.readOnly = true;
                inputMaxAge.value = premium.maxAge;
                divMaxAge.appendChild(inputMaxAge);

                //PREMIUM
                const divPremium = document.createElement("div");
                divPremium.className = "body-element center col-8";
                const inputPremium = document.createElement("input");
                inputPremium.className = "body-box";
                inputPremium.type = "Text";
                inputPremium.readOnly = true;
                inputPremium.value = premium.premium;
                divPremium.appendChild(inputPremium);

                //BUTTON
                const divButton = document.createElement("div");
                divButton.className = "body-element center col-8";
                const inputHidden = document.createElement("input");
                inputHidden.className = "hidden";
                inputHidden.hidden = true;
                inputHidden.value = premium.id;
                divButton.appendChild(inputHidden);
                const inputButton = document.createElement("input");
                inputButton.className = "deletebutton";
                inputButton.type = "button";
                inputButton.value = "Delete";
                inputButton.onclick = deleteRule;
                divButton.appendChild(inputButton);

                //APPEND NEW DIV
                liUnit.appendChild(divCarrier);
                liUnit.appendChild(divPlan);
                liUnit.appendChild(divState);
                liUnit.appendChild(divMonth);
                liUnit.appendChild(divMinAge);
                liUnit.appendChild(divMaxAge);
                liUnit.appendChild(divPremium);
                liUnit.appendChild(divButton);

                ulElement.appendChild(liUnit);

                divToAppend.appendChild(ulElement);
            });
        }
    });
}