class WizardStep2Model {
    constructor(parent) {
        this.parent = parent;

        this.reportId = ko.observable(0);
        this.availableTables = ko.observableArray([]);
        this.selectedTables = ko.observableArray([]);
    }

    save = () => {
        return fetch("/report-builder/save-wizard-step-2", {
            method: "POST",
            headers: {
                'Content-type': 'application/json; charset=utf-8'
            },
            body: ko.toJSON({
                ReportId: this.reportId,
                SelectedTables: this.selectedTables
            })
        })
        .then(response => response.json())
        .then(json => {
            if (json.success) {
                this.parent.step3.reportId(json.model.reportId);
                this.parent.step3.availableColumns([]);
                this.parent.step3.selectedColumns([]);
                this.parent.step3.availableEnumerations([]);
                this.parent.step3.availableTransformFunctions([]);

                json.model.availableColumns.forEach(item => {
                    this.parent.step3.availableColumns.push({
                        name: item.columnName,
                        type: item.type,
                        isForeignKey: item.isForeignKey,
                        availableParentColumns: item.availableParentColumns
                    });
                });

                json.model.availableEnumerations.forEach(item => {
                    this.parent.step3.availableEnumerations.push({ id: item.id, name: item.name });
                });

                json.model.availableTransformFunctions.forEach(item => {
                    this.parent.step3.availableTransformFunctions.push({ name: item });
                });

                json.model.selectedColumns.forEach(item => {
                    this.parent.step3.selectedColumns.push(new SelectedColumnModel(
                        item.columnName,
                        item.type,
                        item.alias,
                        item.isLiteral,
                        item.displayColumn,
                        item.enumerationId,
                        item.transformFunction,
                        item.format,
                        item.isHidden
                    ));
                });

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