class WizardStep3Model {
    constructor(parent) {
        this.parent = parent;

        this.reportId = ko.observable(0);
        this.availableColumns = ko.observableArray([]);
        this.selectedColumns = ko.observableArray([]);
        this.availableEnumerations = ko.observableArray([]);
        this.availableTransformFunctions = ko.observableArray([]);

        this.validator = false;
    }

    init = () => {
        this.validator = $("#wizard-form-3").validate();
    }

    addColumn = () => {
        this.selectedColumns.push(new SelectedColumnModel(null, null, null, false, null, null, null, null, false));
        const index = this.selectedColumns().length - 1;
        this.column_onChange(index);
    };

    addLiteral = () => {
        this.selectedColumns.push(new SelectedColumnModel(null, null, null, true, null, null, null, null, false));
    };

    removeColumn = () => {
        this.selectedColumns.remove(this);
    };

    removeAllColumns = () => {
        if (confirm("Are you sure you want to remove ALL selected columns?")) {
            this.selectedColumns([]);
        }
    };

    column_onChange = (index) => {
        const currentColumn = this.selectedColumns()[index];

        // Get column from availableColumns
        const col = ko.utils.arrayFirst(this.availableColumns(), function (item) {
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
            col.availableParentColumns.forEach(item => {
                select.append($('<option>', {
                    value: item,
                    text: item
                }));
            });
            select.show();
        }
    };

    save = () => {
        if (!$("#wizard-form-3").valid()) {
            return false;
        }

        // Ensure unique columns
        const nonLiteralColumns = $.grep(this.selectedColumns(), function (element) { return element.isLiteral == false; });
        const selectedColumns = nonLiteralColumns.map(function (col) {
            return col.columnName;
        });

        if (hasDuplicates(selectedColumns)) {
            $.notify('One or columns is selected multiple times. Please remove duplicates.', "error");
            console.log('One or columns is selected multiple times. Please remove duplicates.');
            return false;
        }

        const formData = form2js('wizard-form-3', '.', true);

        return fetch("/report-builder/save-wizard-step-3", {
            method: "POST",
            headers: {
                'Content-type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify(formData)
        })
        .then(response => response.json())
        .then(json => {
            if (json.success) {
                this.parent.step4.init(json.model);
                return true;
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
                return false;
            }
        })
        .catch(error => {
            $.notify(this.parent.parent.translations.updateRecordError, "error");
            console.error('Error: ', error);
            return false;
        });
    };
}