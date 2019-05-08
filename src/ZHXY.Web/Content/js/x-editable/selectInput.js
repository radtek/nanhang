(function ($) {
    "use strict";

    var SelectInput = function (options) {
        this.init('selectInput', options, SelectInput.defaults);
    };

    //inherit from Abstract input
    $.fn.editableutils.inherit(SelectInputs, $.fn.editabletypes.abstractinput);

    $.extend(SelectInput.prototype, {
        /**
        Renders input from tpl

        @method render()
        **/
        render: function () {
            this.$input = this.$tpl.find('input');
        },

        /**
        Default method to show value in element. Can be overwritten by display option.

        @method value2html(value, element)
        **/
        value2html: function (value, element) {
            if (!value) {
                $(element).empty();
                return;
            }
            var html = $('<div>').text(value.F_id).html() + ', ' + $('<div>').text(value.F_Name).html() + '' + $('<div>').text(value.name).html();
            $(element).html(html);
        },

        /**
        Gets value from element's html

        @method html2value(html)
        **/
        html2value: function (html) {
            /*
              you may write parsing method to get value by element's html
              e.g. "Moscow, st. Lenina, bld. 15" => {city: "Moscow", street: "Lenina", building: "15"}
              but for complex structures it's not recommended.
              Better set value directly via javascript, e.g.
              editable({
                  value: {
                      city: "Moscow",
                      street: "Lenina",
                      building: "15"
                  }
              });
            */
            return null;
        },

        /**
         Converts value to string.
         It is used in internal comparing (not for sending to server).

         @method value2str(value)
        **/
        value2str: function (value) {
            var str = '';
            if (value) {
                for (var k in value) {
                    str = str + k + ':' + value[k] + ';';
                }
            }
            return str;
        },

        /*
         Converts string to value. Used for reading value from 'data-value' attribute.

         @method str2value(str)
        */
        str2value: function (str) {
            /*
            this is mainly for parsing value defined in data-value attribute.
            If you will always set value by javascript, no need to overwrite it
            */
            return str;
        },

        /**
         Sets value of input.

         @method value2input(value)
         @param {mixed} value
        **/
        value2input: function (value) {
            if (!value) {
                return;
            }
            this.$input.filter('[name="city"]').val(value.city);
            this.$input.filter('[name="street"]').val(value.street);
            this.$input.filter('[name="building"]').val(value.building);
        },

        /**
         Returns value of input.

         @method input2value()
        **/
        input2value: function () {
            return {
                city: this.$input.filter('[name="city"]').val(),
                street: this.$input.filter('[name="street"]').val(),
                building: this.$input.filter('[name="building"]').val()
            };
        },

        /**
        Activates input: sets focus on the first field.

        @method activate()
       **/
        activate: function () {
            this.$input.filter('[name="city"]').focus();
        },

        /**
         Attaches handler to submit form in case of 'showbuttons=false' mode

         @method autosubmit()
        **/
        autosubmit: function () {
            this.$input.keydown(function (e) {
                if (e.which === 13) {
                    $(this).closest('form').submit();
                }
            });
        }
    });

    SelectInput.defaults = $.extend({}, $.fn.editabletypes.abstractinput.defaults, {
        tpl: '<div class="editable-content"><label><span>ID: </span><input disabled type="text" name="id" class="input-small"></label></div>' +
            '<div class="editable-content"><label><span>字典: </span><select type="text" name="street" class="input-small"></select></div>' +
            '<div class="editable-content"><label><span>名称: </span><input type="text" name="building" class="input-small"></label></div>',

        inputclass: ''
    });

    $.fn.editabletypes.selectInput = SelectInput;
}(window.jQuery));