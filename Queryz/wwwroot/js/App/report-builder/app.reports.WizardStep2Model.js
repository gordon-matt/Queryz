var WizardStep2Model = function (parent) {
    const self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.availableTables = ko.observableArray([]);
    self.selectedTables = ko.observableArray([]);

    self.save = function () {
        let success = false;

        $.ajax({
            url: "/report-builder/save-wizard-step-2",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            //data: JSON.stringify($('#wizard-form-2').serializeObject()),
            data: ko.toJSON({
                ReportId: self.reportId,
                SelectedTables: self.selectedTables
            }),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step3.reportId(json.model.reportId);
                self.parent.step3.availableColumns([]);
                self.parent.step3.selectedColumns([]);
                self.parent.step3.availableEnumerations([]);
                self.parent.step3.availableTransformFunctions([]);

                $.each(json.model.availableColumns, function () {
                    const column = this;
                    self.parent.step3.availableColumns.push({
                        name: column.columnName,
                        type: column.type,
                        isForeignKey: column.isForeignKey,
                        availableParentColumns: column.availableParentColumns
                    });
                });

                $.each(json.model.availableEnumerations, function (i, val) {
                    self.parent.step3.availableEnumerations.push({ id: val.id, name: val.name });
                });

                $.each(json.model.availableTransformFunctions, function () {
                    const transformFunction = this;
                    self.parent.step3.availableTransformFunctions.push({ name: transformFunction });
                });

                $.each(json.model.selectedColumns, function (i, selectedColumn) {
                    self.parent.step3.selectedColumns.push(new SelectedColumnModel(
                        selectedColumn.columnName,
                        selectedColumn.type,
                        selectedColumn.alias,
                        selectedColumn.isLiteral,
                        selectedColumn.displayColumn,
                        selectedColumn.enumerationId,
                        selectedColumn.transformFunction,
                        selectedColumn.format,
                        selectedColumn.isHidden
                    ));
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
};