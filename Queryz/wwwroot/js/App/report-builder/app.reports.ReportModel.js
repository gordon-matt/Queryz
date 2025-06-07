class ReportModel {
    constructor(parent) {
        // Ensure parent exists and has translations
        this.parent = parent || { translations: {} };

        this.step1 = new WizardStep1Model(this);
        this.step2 = new WizardStep2Model(this);
        this.step3 = new WizardStep3Model(this);
        this.step4 = new WizardStep4Model(this);
        this.step5 = new WizardStep5Model(this);
        this.step6 = new WizardStep6Model(this);
        this.step7 = new WizardStep7Model(this);
    }

    create = (groupId) => {
        this.step1.id(0);
        this.step1.name(null);
        this.step1.groupId(groupId);
        this.step1.dataSourceId(null);
        this.step1.enabled(true);
        this.step1.emailEnabled(false);

        this.step7.isDistinct(false);
        this.step7.rowLimit(null);
        this.step7.enumerationHandling(0);

        $("#wizard").steps('reset');
        switchSection($("#wizard-section"));
    };

    getTranslations = () => {
        return this.parent?.translations || {};
    }

    edit = (id) => {
        const translations = this.getTranslations();

        fetch(`/report-builder/start-wizard/${id}`)
            .then(response => response.json())
            .then(json => {
                if (json.success) {
                    this.step1.id(json.model.id);
                    this.step1.name(json.model.name);
                    this.step1.groupId(json.model.groupId);
                    this.step1.dataSourceId(json.model.dataSourceId);
                    this.step1.enabled(json.model.enabled);
                    this.step1.emailEnabled(json.model.emailEnabled);

                    $("#wizard").steps('reset');
                    switchSection($("#wizard-section"));
                }
                else {
                    $.notify(json.message, "error");
                    console.log(json.message);
                }
            })
            .catch(error => {
                $.notify(translations.getRecordError, "error");
                console.error('Error: ', error);
            });
    };

    remove = (id) => {
        const translations = this.getTranslations();
        if (confirm(translations.deleteRecordConfirm)) {
            fetch(`${reportApiUrl}(${id})`, { method: 'DELETE' })
                .then(response => {
                    if (response.ok) {
                        $('#Grid').data('kendoGrid').dataSource.read();
                        $('#Grid').data('kendoGrid').refresh();

                        $.notify(translations.deleteRecordSuccess, "success");
                    } else {
                        $.notify(translations.deleteRecordError, "error");
                    }
                })
                .catch(error => {
                    $.notify(translations.deleteRecordError, "error");
                    console.error('Error: ', error);
                });
        }
    };

    cancel() {
        switchSection($("#grid-section"));
    };
}