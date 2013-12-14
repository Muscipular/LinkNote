"use strict";
define(function (_require, _exports, _module) {
    var ko = _require('ko');
    var Note = function (note) {
        this.root = note.root;
        this.noteTitle = ko.observable(note.title);
        var list = note.list || [];
        this.noteList = ko.observableArray(list.map(function (note) {
            return new Note(note);
        }));
        var expanded = ko.observable(false);
        this.selected = ko.observable(false);
        this.expandable = ko.observable(list.length > 0);
        this.expanded = ko.computed({
            read: function () {
                return this.expandable() && expanded();
            },
            write: function (value) {
                expanded(value);
            },
            owner: this
        });
        this.note = note;
    };

    Note.prototype.reload = function () {
        return _require('service').list(this.root ? null : this);
    };

    return _module.exports = _exports = Note;
});
