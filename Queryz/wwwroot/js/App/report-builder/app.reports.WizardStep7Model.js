var WizardStep7Model = function (parent) {
    const self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.isDistinct = ko.observable(false);
    self.rowLimit = ko.observable(0);
    self.enumerationHandling = ko.observable(0);

    self.availableUsers = ko.observableArray([]);
    self.deniedUserIds = ko.observableArray([]);

    self.save = function () {
        let success = false;

        $.ajax({
            url: "/report-builder/save-wizard-step-7",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                ReportId: self.reportId(),
                IsDistinct: self.isDistinct(),
                RowLimit: self.rowLimit(),
                EnumerationHandling: self.enumerationHandling(),
                DeniedUserIds: self.deniedUserIds()
            }),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                success = true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                success = false;
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.parent.translations.UpdateRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
            success = false;
        });

        return success;
    };
};