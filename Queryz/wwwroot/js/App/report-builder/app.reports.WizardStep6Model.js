var WizardStep6Model = function (parent) {
    const self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.availableColumns = ko.observableArray([]);
    self.relationships = ko.observableArray([]);

    self.parentTable_onChange = function (index) {
        const relationship = self.relationships()[index];

        if (relationship.parentTable) {
            $.ajax({
                url: `/report-builder/get-columns/${self.reportId()}/${relationship.parentTable}`,
                type: "GET",
                dataType: "json",
                async: false
            })
            .done(function (json) {
                if (json.success) {
                    const select = $(`select[name="Relationships[${index}].PrimaryKey"]`);
                    select.find('option').remove();

                    $(json.columns).each(function (i, val) {
                        select.append($('<option>', {
                            value: val,
                            text: val,
                            selected: (relationship.primaryKeyBackup && val == relationship.primaryKeyBackup)
                        }));
                    });
                }
                else {
                    $.notify(json.message, "error");
                    console.log(json.message);
                }
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                console.log(textStatus + ': ' + errorThrown);
            });
        }

        //const element = $(event.currentTarget).next();
    };

    self.save = function () {
        let success = false;

        const formData = form2js('wizard-form-6', '.', true);

        $.ajax({
            url: "/report-builder/save-wizard-step-6",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(formData),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step7.reportId(json.model.reportId);
                self.parent.step7.isDistinct(json.model.isDistinct);
                self.parent.step7.rowLimit(json.model.rowLimit);
                self.parent.step7.enumerationHandling(json.model.enumerationHandling);

                self.parent.step7.availableUsers([]);
                self.parent.step7.deniedUserIds([]);

                const hasAvailableUsers = json.model.availableUsers && json.model.availableUsers.length > 0;

                if (hasAvailableUsers > 0) {
                    $.each(json.model.availableUsers, function () {
                        self.parent.step7.availableUsers.push(this);
                    });
                }

                if (json.model.deniedUserIds && json.model.deniedUserIds.length > 0) {
                    $.each(json.model.deniedUserIds, function () {
                        self.parent.step7.deniedUserIds.push(this);
                    });
                }

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