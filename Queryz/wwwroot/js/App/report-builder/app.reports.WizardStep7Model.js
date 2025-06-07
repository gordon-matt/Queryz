class WizardStep7Model {
    constructor(parent) {
        this.parent = parent;

        this.reportId = ko.observable(0);
        this.isDistinct = ko.observable(false);
        this.rowLimit = ko.observable(0);
        this.enumerationHandling = ko.observable(0);

        this.availableUsers = ko.observableArray([]);
        this.deniedUserIds = ko.observableArray([]);
    }

    save = () => {
        return fetch("/report-builder/save-wizard-step-7", {
            method: "POST",
            headers: {
                'Content-type': 'application/json; charset=utf-8'
            },
            body: JSON.stringify({
                ReportId: this.reportId(),
                IsDistinct: this.isDistinct(),
                RowLimit: this.rowLimit(),
                EnumerationHandling: this.enumerationHandling(),
                DeniedUserIds: this.deniedUserIds()
            })
        })
        .then(response => response.json())
        .then(json => {
            if (json.success) {
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