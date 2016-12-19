
var SPCShippingOptions = {
        myform: false,
        myurlSaveProduct: false,
        //myurlSendProduct: false,
        //myurlCkeckProduct: false,
        //myurlPriceProduct: false,
        //myurlStockProduct: false,
        //myurlPictureProduct: false,

        init: function (mform, sUrl) {
            this.myform = mform;
            this.myurlSaveProduct = sUrl;
            //this.myurlSendProduct = sendUrl;
            //this.myurlPriceProduct = priceUrl;
            //this.myurlStockProduct = stockUrl;
            //this.myurlPictureProduct = pictureUrl;
            //this.myurlCkeckProduct = checkUrl;
            //var feedModel = kendo.observable({
            //    feedId: "51603017028",
            //    asin: "B00HQFQIZS",
            //    fieldsEnabled: function () {
            //        return this.get("feedId").length > 0 || this.get("asin").length > 0;
            //    }
            //});
            //kendo.bind($('#AmazonProduct-form'), feedModel);
        },

        SaveProduct: function () {
            $('.message-box.message-box-error').html('').hide();
            $('.message-box.message-box-success').html('').hide();
            $.ajax({
                cache: false,
                url: this.myurlSaveProduct,
                data: $(this.myform).serialize(),
                type: 'post',
                success: this.ajaxSuccess,
                error: this.ajaxFailure
            });
        },

        //SendProduct: function () {
        //    if ($('#FeedSubmissionId').val() != '') {
        //        var agree = confirm('Item will be sent once again to Amazon, are you sure?');
        //        if (agree == false)
        //            return false;
        //    }
        //    $('.message-box.message-box-error').html('').hide();
        //    $('.message-box.message-box-success').html('').hide();
        //    $.ajax({
        //        cache: false,
        //        url: this.myurlSendProduct,
        //        data: $(this.myform).serialize(),
        //        type: 'post',
        //        success: this.nextStep,
        //        error: this.ajaxFailure
        //    });
        //},

        //CheckStatusProduct: function () {
        //    $.ajax({
        //        cache: false,
        //        url: this.myurlCkeckProduct,
        //        data: $(this.myform).serialize(),
        //        type: 'post',
        //        success: this.nextStep,
        //        error: this.ajaxFailure
        //    });
        //},

        //UpdatePriceProduct: function () {
        //    $.ajax({
        //        cache: false,
        //        url: this.myurlPriceProduct,
        //        data: $(this.myform).serialize(),
        //        type: 'post',
        //        success: this.nextStep,
        //        error: this.ajaxFailure
        //    });
        //},
        //UpdateStockProduct: function () {
        //    $.ajax({
        //        cache: false,
        //        url: this.myurlStockProduct,
        //        data: $(this.myform).serialize(),
        //        type: 'post',
        //        success: this.nextStep,
        //        error: this.ajaxFailure
        //    });
        //},
        //UpdatePictureProduct: function () {
        //    $.ajax({
        //        cache: false,
        //        url: this.myurlPictureProduct,
        //        data: $(this.myform).serialize(),
        //        type: 'post',
        //        success: this.nextStep,
        //        error: this.ajaxFailure
        //    });
        //},

        ajaxSuccess: function () {
            alert('Shipping Options have been saved.');
        },
        ajaxFailure: function () {
            alert('Failed to perform request. Please refresh the page and try again.');
        },

        //nextStep: function (response) {
        //    if (response.feedsubmissionId) {
        //        $('#FeedSubmissionId').val(response.feedsubmissionId.toString()).change();
        //    }
        //    if (response.message) {
        //        $('.message-box.message-box-success').show().text(response.message);
        //    }
        //    if (typeof response.errors != 'undefined' && response.errors.length) {
        //        var errorBox = $("<ul>");
        //        //alert(response.message);
        //        var errorData = $.map(response.errors, function (v, i) {
        //            return $("<li>").text(v);
        //        });
        //        $.each(errorData, function (i, v) { errorBox.append(v); });
        //        console.log(errorData.join(''));

        //        $('.message-box.message-box-error').show().html(errorBox);
        //    }
        //    if (response.success)
        //        $('.message-box.message-box-success').show().append($('<div>').text('Status Ok, no error messages. Please check the product on Amazon.'));
        //    //  now the amazon response messages
        //    if (typeof response.messages != 'undefined' && response.messages.length) {
        //        var messageBox = $("<table>", { border: "1" });
        //        var headers = $.map(['ID', 'Type', 'SKU', 'Code', 'Content'], function (v, i) {
        //            return $('<th>').text(v);
        //        });
        //        var headerRow = $('<tr class="form-group">');
        //        $.each(headers, function (i, v) { headerRow.append(v) });
        //        messageBox.append(headerRow);
        //        var errorData = $.map(response.messages, function (v, i) {
        //            var row = $('<tr class="form-group">');
        //            var values = $.map([v.MessageId, (v.MessageType > 0 ? 'Error' : 'Warning'), v.ProductSKU, v.Code, v.MessageText], function (value, index) {
        //                console.log(value);
        //                return $('<td>').text(value);
        //            });
        //            $.each(values, function (ii, vv) {
        //                console.log($(vv));
        //                row.append($(vv));
        //            });
        //            return row;
        //        });
        //        $.each(errorData, function (i, v) { messageBox.append(v); });
        //        console.log(errorData.join(''));

        //        if (response.success)
        //            $('.message-box.message-box-success').show().append(messageBox);
        //        else
        //            $('.message-box.message-box-error').show().append(messageBox);
        //    }
        //}
    }

SPCShippingOptions.init('#shipping-byweightextended-form', '/store/ProductAdmin/Save');

