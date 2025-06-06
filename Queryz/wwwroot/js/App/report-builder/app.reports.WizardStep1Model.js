var WizardStep1Model = function (parent) {
    const self = this;
    self.parent = parent;

    self.id = ko.observable(0);
    self.name = ko.observable(null);
    self.groupId = ko.observable(0);
    self.dataSourceId = ko.observable(0);
    self.enabled = ko.observable(false);
    self.emailEnabled = ko.observable(false);

    self.validator = $("#wizard-form-1").validate();

    self.availableGroups = ko.observableArray([]);
    self.availableDataSources = ko.observableArray([]);

    self.save = function () {
        if (!$("#wizard-form-1").valid()) {
            return false;
        }

        let success = false;

        $.ajax({
            url: "/report-builder/save-wizard-step-1",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({
                Id: self.id(),
                Name: self.name(),
                GroupId: self.groupId(),
                DataSourceId: self.dataSourceId(),
                Enabled: self.enabled(),
                EmailEnabled: self.emailEnabled()
            }),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step2.reportId(json.model.reportId);
                self.parent.step2.availableTables([]);
                self.parent.step2.selectedTables([]);
                $.each(json.model.availableTables, function () {
                    self.parent.step2.availableTables.push(this);
                });
                $.each(json.model.selectedTables, function () {
                    self.parent.step2.selectedTables.push(this);
                });

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

    self.reloadGroups = function () {
        // We use timeout to prevent error when both the grid and this try to call the same url at the same time. It causes
        //  a NotSupportedException on the db context
        setTimeout(function () {
            $.ajax({
                url: reportGroupApiUrl,
                type: "GET",
                dataType: "json",
                async: false
            })
            .done(function (json) {
                self.availableGroups([]);
                $.each(json.value, function () {
                    const group = this;
                    self.availableGroups.push({ id: group.Id, name: group.Name });
                });
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify("Could not reload groups", "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }, 1000);
    };

    self.reloadDataSources = function () {
        // We use timeout to prevent error when both the grid and this try to call the same url at the same time. It causes
        //  a NotSupportedException on the db context
        setTimeout(function () {
            $.ajax({
                url: `${dataSourceApiUrl}?$orderby=Name`,
                type: "GET",
                dataType: "json",
                async: false
            })
            .done(function (json) {
                self.availableDataSources([]);
                $.each(json.value, function () {
                    const dataSource = this;
                    self.availableDataSources.push({ id: dataSource.Id, name: dataSource.Name });
                });
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify("Could not reload data sources", "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }, 1000);
    };
};