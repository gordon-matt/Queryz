var WizardStep5Model = function (parent) {
    const self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.availableColumns = ko.observableArray([]);
    self.sortings = ko.observableArray([]);

    self.validator = $("#wizard-form-5").validate();

    self.addSorting = function () {
        self.sortings.push({
            columnName: null,
            sortDirection: 0
        });
    };
    self.removeSorting = function () {
        self.sortings.remove(this);
    };

    self.save = function () {
        if (!$("#wizard-form-5").valid()) {
            return false;
        }

        let success = false;

        const formData = form2js('wizard-form-5', '.', true);

        $.ajax({
            url: "/report-builder/save-wizard-step-5",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(formData),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step6.reportId(json.model.reportId);
                self.parent.step6.relationships([]);
                $.each(json.model.relationships, function () {
                    const relationship = this;
                    self.parent.step6.relationships.push(new RelationshipModel(
                        relationship.tableName,
                        relationship.parentTable,
                        relationship.primaryKey,
                        relationship.foreignKey,
                        relationship.joinType,
                        relationship.availableColumns,
                        relationship.availableParentTables
                    ));
                });


                const relationCount = json.model.relationships.length;
                for (let i = 0; i < relationCount; i++) {
                    self.parent.step6.parentTable_onChange(i);
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