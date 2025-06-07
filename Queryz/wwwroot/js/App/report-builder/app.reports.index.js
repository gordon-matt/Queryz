'use strict'

const dataSourceApiUrl = "/odata/DataSourceApi";
const enumerationApiUrl = "/odata/EnumerationApi";
const reportGroupApiUrl = "/odata/ReportGroupApi";
const reportApiUrl = "/odata/ReportApi";

class ViewModel {
    constructor() {
        this.dataSourceModel = new DataSourceModel(this);
        this.enumerationModel = new EnumerationModel(this);
        this.reportGroupModel = new ReportGroupModel(this);
        this.reportModel = new ReportModel(this);

        this.gridPageSize = 10;
        this.translations = false;
    }

    init = () => {
        currentSection = $("#grid-section");

        // Load translations first, else will have errors
        fetch("/report-builder/get-translations")
            .then(response => response.json())
            .then(json => {
                this.translations = json;
                this.gridPageSize = $("#GridPageSize").val();

                this.dataSourceModel.init();
                this.enumerationModel.init();
                this.reportGroupModel.init();
            })
            .catch(error => {
                console.error('Error: ', error);
            });
    };

    showDataSources() {
        switchSection($("#dataSources-grid-section"));
    };

    showEnumerations() {
        switchSection($("#enumerations-grid-section"));
    };
}

var viewModel;
$(document).ready(function () {
    // Initialize Wizard
    $("#wizard").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        stepsOrientation: "vertical",
        onStepChanging: function (event, currentIndex, newIndex) {
            if (currentIndex == newIndex) {
                // This happens when calling reset
                return false;
            }

            if (newIndex < currentIndex) {
                // Always allow previous step
                return true;
            };

            switch (currentIndex) {
                case 0: return viewModel.reportModel.step1.save();
                case 1: return viewModel.reportModel.step2.save();
                case 2: return viewModel.reportModel.step3.save();
                case 3: return viewModel.reportModel.step4.save();
                case 4: return viewModel.reportModel.step5.save();
                case 5: return viewModel.reportModel.step6.save();
                //case 6: return viewModel.reportModel.step7.save(); // This one is in 'onFinishing' function
                default: alert("Unknown step at index: " + currentIndex); return false;
            }
        },
        onFinishing: function (event, currentIndex) {
            //return viewModel.reportModel.step8.save();
            return viewModel.reportModel.step7.save();
        },
        onFinished: function (event, currentIndex) {
            GridHelper.refreshGrid();
            switchSection($("#grid-section"));
        }
    });

    viewModel = new ViewModel();
    ko.applyBindings(viewModel);
    viewModel.init();

    // Load Groups to dropdown
    viewModel.reportModel.step1.reloadGroups();
    viewModel.reportModel.step1.reloadDataSources();

    viewModel.reportModel.step1.groupId(($("#Report_GroupId option:first").val())); // Hack
    viewModel.reportModel.step1.dataSourceId(($("#Report_DataSourceId option:first").val()));

    $(document).on('click', 'td.clickable', function () {
        $(this).find('i').toggleClass('fa-plus fa-minus');
    });
});

$.fn.hasAttr = function (name) {
    const attr = this.attr(name);
    // For some browsers, `attr` is undefined; for others,
    // `attr` is false.  Check for both.
    return typeof attr !== typeof undefined && attr !== false;
};

ko.bindingHandlers.sortable.options.start = function (event, ui) {
    ui.item.toggleClass("highlight");
};

ko.bindingHandlers.sortable.options.stop = function (event, ui) {
    ui.item.toggleClass("highlight");
};

function lowerCase(str) {
    return str.toLowerCase();
}

function upperCase(str) {
    return str.toUpperCase();
}

function camelCase(str) {
    str = replaceAccents(str);
    str = removeNonWord(str)
        .replace(/\-/g, ' ') //convert all hyphens to spaces
        .replace(/\s[a-z]/g, upperCase) //convert first char of each word to UPPERCASE
        .replace(/\s+/g, '') //remove spaces
        .replace(/^[A-Z]/g, lowerCase); //convert first char to lowercase
    return str;
}

function unCamelCase(str) {
    str = str.replace(/([a-z\xE0-\xFF])([A-Z\xC0\xDF])/g, '$1 $2');
    str = str.toLowerCase(); //add space between camelCase text
    return str;
}

function pascalCase(str) {
    return camelCase(str).replace(/^[a-z]/, upperCase);
}

function titleCase(str) {
    str = str.toLowerCase().split(' ');

    let final = [];
    for (let word of str) {
        final.push(word.charAt(0).toUpperCase() + word.slice(1));
    }
    return final.join(' ')
}

function hasDuplicates(array) {
    return (new Set(array)).size !== array.length;
}