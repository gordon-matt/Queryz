'use strict'

const ViewModel = function () {
    const self = this;

    self.reportId = ko.observable(0);
    
    self.translations = false;
    
    self.init = function () {
        // Load translations first, else will have errors

        fetch("/report-builder/get-translations")
        .then(response => response.json())
        .then(json => {
            self.translations = json;
        })
        .catch(error => {
            console.error('Error: ', error);
        });

        self.reportId(model.ReportId);

        $('#query-builder').queryBuilder(model.JQQueryBuilderConfig);

        $('#query-builder').on('getSQLFieldID.queryBuilder.filter', function (e) {
            if (e.value in model.JQQueryBuilderFieldIdMappings) {
                e.value = model.JQQueryBuilderFieldIdMappings[e.value];
            }
        });

        if (model.Query && model.Query != "null") {
            //$('#query-builder').queryBuilder('setRulesFromSQL', model.Query);
            $('#query-builder').queryBuilder('setRules', JSON.parse(model.Query));
        }
    };
    
    self.submit = function () {
        const result = $('#query-builder').queryBuilder('getSQL', false);
        
        $('#loading').show();
        $('#wizard').hide();

        return fetch("/report-builder/preview", {
            method: "POST",
            headers: {
                "Content-Type": "application/json; charset=utf-8",
            },
            body: JSON.stringify({
                ReportId: self.reportId(),
                Query: result?.sql
            })
        })
        .then(response => response.json())
        .then(json => {
            if (json.success) {
                $("#wizard-p-1").html(json.html);
                return true;
            }
            else {
                $.notify(json.message, "error");
                return false;
            }
        })
        .catch(error => {
            $.notify("Error when trying to run report!", "error");
            console.error('Error: ', error);
            return false;
        })
        .finally(() => {
            $('#loading').hide();
            $('#wizard').show();
        });
    };
};

var viewModel;
let isProgrammaticStepChange = false; // prevent infinite loop on step change
$(document).ready(function () {
    $("#wizard").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        stepsOrientation: "vertical",
        enableFinishButton: false,
        onStepChanging: function (event, currentIndex, newIndex) {
            if (isProgrammaticStepChange) {
                isProgrammaticStepChange = false; // Reset flag
                return true; // Allow programmatic step changes
            }

            if (newIndex < currentIndex) {
                // Always allow previous step
                return true;
            };

            viewModel.submit().then((success) => {
                if (success) {
                    isProgrammaticStepChange = true;
                    $("#wizard").steps("next"); // Manually move to next step
                }
            });

            return false; // Block for now
        },
        onFinishing: function (event, currentIndex) {
            return true;
        },
        onFinished: function (event, currentIndex) {
            //window.location.href = '/report-builder';
        }
    });

    viewModel = new ViewModel();
    ko.applyBindings(viewModel);
    viewModel.init();
});
