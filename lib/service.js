"use strict";
var path = require('path');
var util = require('util');
var Promise = require('bluebird');
var fs = Promise.promisifyAll(require('fs'));
var marked = require('marked');
var hljs = require('highlight.js');

marked.setOptions({
    highlight: function (code, lang) {
        try {
            return hljs.highlight(lang, code).value;
        }
        catch (e) {
            //logger.error(e, lang + ' does not exists.');
            return code;
        }
    }
});


var Service = function () {
    Service.super_.call(this);
};

var _defaultNote = {
    title: '',
    content: '',
    contentType: 'md',
    contentMode: 'file',
    meta: '',
    attachment: [],
    encrypted: false
};

var extend = function () {
    if (arguments.length < 2) {
        return arguments[0];
    }
    var o = arguments[0], x = null , len = arguments.length, i = 1, ns = null;
    for (; i < len; i++) {
        ns = Object.getOwnPropertyNames((x = arguments[i]));
        for (var i2 = 0, len2 = ns.length; i2 < len2; i2++) {
            var x2 = x[ns[i2]];
            if (x2 !== undefined && x2 !== null) {
                o[ns[i2]] = x2;
            }
        }
    }
    return o;
};

util.inherits(Service, require('events').EventEmitter);

Service.prototype.init = function () {
    var _path = this._repoDir = path.join(__dirname, '..', 'repo');
    if (!fs.existsSync(_path)) {
        fs.mkdirSync(_path);
    }
    while (!fs.existsSync(_path)) {
    }
    this.emit('init');
    return this;
};

Service.prototype.__defineGetter__('repoDir', function () {
    return this._repoDir;
});

Service.prototype.__defineSetter__('repoDir', function (value) {
    var self = this;
    if (value !== self._repoDir) {
        if (self._repoDir === undefined) {
            process.nextTick(function () {
                self.emit('setting', {key: 'repoDir', value: value});
            });
        }
        self._repoDir = value;
        self.list(value).then(function (result) {
            process.nextTick(function () {
                self.emit('changed', result);
            });
        });
    }
    return this;
});

Service.prototype.list = function (note, excludeChild) {
    var self = this;
    var relativePath = note ? note.path : '';
    var absolutePath = path.join(this.repoDir, relativePath);
    return fs.readdirAsync(absolutePath)
        .filter(function (file) {
            try {
                return !(/^\./.test(file)) && fs.statSync(path.join(absolutePath, file)).isDirectory();
            } catch (e) {
                console.log(e);
                return false;
            }
        })
        .map(function (file) {
            return fs.readFileAsync(path.join(absolutePath, file, 'note.json'), 'utf8')
                .then(function (data) {
                    var note = JSON.parse(data);
                    note.path = path.join(relativePath, file);
                    note = extend({}, _defaultNote, note);
                    return note;
                })
                .catch(function (e) {
                    console.log(e);
                    return undefined;
                });
        })
        .filter(function (f) {
            return f !== undefined;
        })
        .map(function (note) {
            if (excludeChild) {
                return note;
            }
            return self
                .list(note)
                .then(function (notes) {
                    note.list = notes;
                    return note;
                })
        })
};

Service.prototype.loadContent = function (note) {
    var repoDir = this._repoDir;
    if (note.contentMode === "inner") {
        return Promise.resolve(note.content);
    }
    return fs.readFileAsync(path.join(repoDir, note.path, note.content), {encoding: 'utf8'})
        .then(function (data) {
            return data;
        })
        .catch(function (e) {
            console.log(e);
            return '';
        });
};

Service.prototype.md = function (md) {
    return marked(md);
};

Service.prototype.save = function (note, content) {
    var repoDir = this._repoDir;
    if (note.contentMode === 'inner') {
        note.contentMode = 'file';
        note.content = 'content.md'
    }
    note = extend({}, note);
    delete note.list;
    return fs.writeFileAsync(path.join(repoDir, note.path, 'note.json'), JSON.stringify(note), {encoding: 'utf8'})
        .return(fs.writeFileAsync(path.join(repoDir, note.path, note.content), content, {encoding: 'utf8'}))
};


module.exports = new Service();


