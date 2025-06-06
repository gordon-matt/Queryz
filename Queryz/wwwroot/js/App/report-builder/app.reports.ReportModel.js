const ReportModel = function (parent) {
    const self = this;
    self.parent = parent;

    self.step1 = new WizardStep1Model(self);
    self.step2 = new WizardStep2Model(self);
    self.step3 = new WizardStep3Model(self);
    self.step4 = new WizardStep4Model(self);
    self.step5 = new WizardStep5Model(self);
    self.step6 = new WizardStep6Model(self);
    self.step7 = new WizardStep7Model(self);

    self.create = function (groupId) {
        self.step1.id(0);
        self.step1.name(null);
        self.step1.groupId(groupId);
        self.step1.dataSourceId(null);
        self.step1.enabled(true);
        self.step1.emailEnabled(false);

        self.step7.isDistinct(false);
        self.step7.rowLimit(null);
        self.step7.enumerationHandling(0);

        $("#wizard").steps('reset');
        switchSection($("#wizard-section"));
    };

    self.edit = function (id) {
        $.ajax({
            url: `/report-builder/start-wizard/${id}`,
            type: "GET",
            dataType: "json",
            async: false
        })
        .done(function (json) {
            if (json.success) {
                self.step1.id(json.model.id);
                self.step1.name(json.model.name);
                self.step1.groupId(json.model.groupId);
                self.step1.dataSourceId(json.model.dataSourceId);
                self.step1.enabled(json.model.enabled);
                self.step1.emailEnabled(json.model.emailEnabled);

                $("#wizard").steps('reset');
                switchSection($("#wizard-section"));
            }
            else {
                $.notify(json.message, "error");
                console.log(json.message);
            }
        })
        .fail(function (jqXHR, textStatus, errorThrown) {
            $.notify(self.parent.translations.getRecordError, "error");
            console.log(textStatus + ': ' + errorThrown);
        });
    };
    self.remove = function (id) {
        if (confirm(self.parent.translations.deleteRecordConfirm)) {
            $.ajax({
                url: `${reportApiUrl}(${id})`,
                type: "DELETE",
                async: false
            })
            .done(function (json) {
                $('#Grid').data('kendoGrid').dataSource.read();
                $('#Grid').data('kendoGrid').refresh();

                $.notify(self.parent.translations.deleteRecordSuccess, "success");
            })
            .fail(function (jqXHR, textStatus, errorThrown) {
                $.notify(self.parent.translations.deleteRecordError, "error");
                console.log(textStatus + ': ' + errorThrown);
            });
        }
    };
    self.cancel = function () {
        switchSection($("#grid-section"));
    };
};