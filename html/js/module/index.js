define(function (require) {
    var Note = require('./note.js');
    var $ = require('$');
    var ko = require('ko');
    var service = require('service');
    var vm = {
        init: function () {
            this.selectedNote = ko.observableArray();
        },
        reload: function () {
            this.root.reload();
        },
        edit: function () {
            var preview = this.preview();
            var data = $('.editor')[0].innerText;
            this.preview(!preview);
            if (!preview) {
                $('.preview').html(service.md(data));
            }
        },
        save: function () {
            var selectedNote = this.selectedNote()[0];
            if (selectedNote) {
                service.save(selectedNote.note, $('.editor')[0].innerText)
            }
        },
        preview: ko.observable(true)
    };
    vm.init();

//    require('template')
//        .loadTemplate('note-list')
//        .then(function () {
//            return require('service').list();
//        })
//        .map(function (note) {
//            return new Note(note);
//        })
    require('service').list()
        .then(function (notes) {
            return new Note({title: 'root', list: notes, root: true});
        })
        .then(function (root) {
            vm.root = root;
        })
        .then(function () {
            ko.applyBindings(vm);
        });

    var expand = function (e) {
        var data = ko.dataFor(e.target);
        if (data) {
            data.expanded(!data.expanded());
        }
    };
    $('.left-panel > div')
        .on('dblclick', '.title', expand)
        .on('click', '.expander', expand)
        .on('click', ".title", function (e) {
            var note = ko.dataFor(e.target);
            if (note === vm.selectedNote()[0]) {
                return;
            }
            vm.selectedNote([note]);
            vm.preview(true);
            require('service').loadContent(note.note)
                .then(function (data) {
                    $('.editor').text(data);
                    $('.preview').html(require('service').md(data));
                })
        });
});
