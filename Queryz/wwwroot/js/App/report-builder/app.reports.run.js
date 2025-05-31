'use strict'

var ViewModel = function () {
    var self = this;

    self.reportId = ko.observable(0);
    
    self.translations = false;
    
    self.init = function () {
        // Load translations first, else will have errors
        $.ajax({
            url: "/report-builder/get-translations",
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            self.translations = json;
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            console.log(textStatus + ': ' + errorThrown);
        });

        self.reportId(model.ReportId);

        $('#query-builder').queryBuilder(model.JQQueryBuilderConfig);

        $('#query-builder').on('getSQLFieldID.queryBuilder.filter', function (e) {
            if (e.value in model.JQQueryBuilderFieldIdMappings) {
                e.value = model.JQQueryBuilderFieldIdMappings[e.value];
            }
        });

        if (model.Query && model.Query != "null") {
            console.log(`model.Query: ${model.Query}`);
            //$('#query-builder').queryBuilder('setRulesFromSQL', model.Query);
            $('#query-builder').queryBuilder('setRules', JSON.parse(model.Query));
        }
    };
    
    self.submit = function () {
        var result = $('#query-builder').queryBuilder('getSQL', false);
        
        var success = false;
        
        $('#loading').show();
        $('#wizard').hide();

        $.ajax({
            url: "/report-builder/preview",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                ReportId: self.reportId(),
                Query: result?.sql
            }),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                $("#wizard-p-1").html(json.html);
                success = true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                success = false;
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify("Error when trying to run report!", "error");
            console.log(textStatus + ': ' + errorThrown);
            success = false;
        });

        $('#loading').hide();
        $('#wizard').show();
        return success;
    };
};

var viewModel;
$(document).ready(function () {
    $("#wizard").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        stepsOrientation: "vertical",
        enableFinishButton: false,
        onStepChanging: function (event, currentIndex, newIndex) {
            if (newIndex < currentIndex) {
                // Always allow previous step
                return true;
            };
            return viewModel.submit();
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
