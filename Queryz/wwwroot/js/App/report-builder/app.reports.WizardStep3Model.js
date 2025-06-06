var WizardStep3Model = function (parent) {
    const self = this;
    self.parent = parent;

    self.reportId = ko.observable(0);
    self.availableColumns = ko.observableArray([]);
    self.selectedColumns = ko.observableArray([]);
    self.availableEnumerations = ko.observableArray([]);
    self.availableTransformFunctions = ko.observableArray([]);

    self.validator = $("#wizard-form-3").validate();
    
    self.addColumn = function () {
        self.selectedColumns.push(new SelectedColumnModel(null, null, null, false, null, null, null, null, false));
        const index = self.selectedColumns().length - 1;
        self.column_onChange(index);
    };
    self.addLiteral = function () {
        self.selectedColumns.push(new SelectedColumnModel(null, null, null, true, null, null, null, null, false));
    };
    self.removeColumn = function () {
        self.selectedColumns.remove(this);
    };

    self.removeAllColumns = function () {
        if (confirm("Are you sure you want to remove ALL selected columns?")) {
            self.selectedColumns([]);
        }
    };

    self.column_onChange = function (index) {
        const currentColumn = self.selectedColumns()[index];

        // Get column from availableColumns
        const col = ko.utils.arrayFirst(self.availableColumns(), function (item) {
            return item.name === currentColumn.columnName;
        });

        if (col == null) {
            return false;
        }

        currentColumn.type(col.type);
        
        let alias = col.name;
        const periodIndex = alias.indexOf('.');
        alias = alias.substring(periodIndex + 1);
        alias = titleCase(unCamelCase(alias));
        currentColumn.alias(alias);

        const select = $('select[name="SelectedColumns[' + index + '].DisplayColumn"]');
        select.find('option').remove();
        select.hide();

        if (col.isForeignKey && col.availableParentColumns && col.availableParentColumns.length > 0) {
            $(col.availableParentColumns).each(function (i, val) {
                select.append($('<option>', {
                    value: val,
                    text: val
                }));
            });
            select.show();
        }
    };

    self.save = function () {
        if (!$("#wizard-form-3").valid()) {
            return false;
        }

        // Ensure unique columns
        const nonLiteralColumns = $.grep(self.selectedColumns(), function (element) { return element.isLiteral == false; })
        const selectedColumns = nonLiteralColumns.map(function (col) {
            return col.columnName;
        });

        if (hasDuplicates(selectedColumns)) {
            $.notify('One or columns is selected multiple times. Please remove duplicates.', "error");
            console.log('One or columns is selected multiple times. Please remove duplicates.');
            return false;
        }

        let success = false;

        const formData = form2js('wizard-form-3', '.', true);

        $.ajax({
            url: "/report-builder/save-wizard-step-3",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(formData),
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.parent.step4.init(json.model);
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