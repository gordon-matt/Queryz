var RelationshipModel = function (tableName, parentTable, primaryKey, foreignKey, joinType, availableColumns, availableParentTables) {
    const self = this;

    self.tableName = tableName;
    self.parentTable = parentTable;
    self.primaryKey = primaryKey; // Why does this keep disappearing??? Is it a reseverd word?
    self.foreignKey = foreignKey;
    self.joinType = joinType;
    self.availableColumns = availableColumns;
    self.availableParentTables = availableParentTables;

    self.primaryKeyBackup = primaryKey; // Have extra primary Key field because other one keeps disappearing.
};