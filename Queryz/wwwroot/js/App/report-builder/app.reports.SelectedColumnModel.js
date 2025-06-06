var SelectedColumnModel = function (columnName, type, alias, isLiteral, displayColumn, enumerationId, transformFunction, format, isHidden) {
    const self = this;

    self.columnName = columnName;
    self.type = ko.observable(type);
    self.alias = ko.observable(alias);
    self.isLiteral = isLiteral;
    self.displayColumn = displayColumn;
    self.enumerationId = enumerationId;
    self.transformFunction = transformFunction;
    self.format = format;
    self.isHidden = isHidden;
};