function uniqId() {
    return Math.round(new Date().getTime() + (Math.random() * 100));
}

function show_alert(message, errorMessage, returnUrl) {
    if (confirm(message)) {
        submitReports(errorMessage, returnUrl);
    } else {
        return false;
    }
}

function addNewReport() {
    var element = $('#last-report');

    var newElement = element.clone();
    newElement.find('select[name^="Supplier"]').attr("id", uniqId());
    newElement.find('select[name^="Order"]').attr("id", uniqId());
    newElement.find('select[name^="Order"]').hide();
    //newElement.find('select[name^="Part"]').attr("id", uniqId());
    //newElement.find('select[name^="Part"]').hide();
    //newElement.find('select[name^="DeliveryNumber"]').attr("id", uniqId());
    //newElement.find('select[name^="DeliveryNumber"]').hide();
    //newElement.find('select[name^="ChargeNumber"]').attr("id", uniqId());
    //newElement.find('select[name^="ChargeNumber"]').hide();
    newElement.find('.attribute-div').empty();
    newElement.find('select[name^="BlockedPart"]').attr("id", uniqId());
    newElement.find('select[name^="BlockedPart"]').parent().parent().attr("hidden", "hidden");
    newElement.find('select[name^="ReworkedPart"]').attr("id", uniqId());
    newElement.find('select[name^="ReworkedPart"]').parent().parent().attr("hidden", "hidden");

    var el = newElement.appendTo($('#reports'));
    element.removeAttr('id');

    //el.find('#blocked-parts-list').find('.row').not(':first').remove();
    //el.find('#reworked-parts-list').find('.row').not(':first').remove();

    //el.find('#blocked-parts-list').children().find('input').val(0);
    //el.find('#blocked-parts-list').children().find('option').not(':first').remove();
    //el.find('#reworked-parts-list').children().find('input').val(0);
    //el.find('#reworked-parts-list').children().find('option').not(':first').remove();
    el.find('#CheckedQuantity').val(0);
    el.find('#input-time').val(0);
    el.find('#input-ok').val(0);
    el.find('#CheckedQuantity').attr("disabled", "disabled");
    el.find('#input-time').attr("disabled", "disabled");
    el.find('#input-nok').val(0);
    el.find('#input-reworked').val(0);

    $('html, body').animate({
        scrollTop: $(document).height()
    }), 3000;
}

function addBlockedRow(element) {
    element = $(element);
    var selectedValue = element.find(":selected").val();

    element.unbind("onchange");
    var row = element.parent().parent().clone();
    row.appendTo(element.parent().parent().parent().parent().find('#blocked-parts-list'));
    row.children().find('input')[0].value = "";
    row.find(`option[value="${selectedValue}"]`).remove();
}

function addReworkRow(element) {
    element = $(element);
    var selectedValue = element.find(":selected").val();

    element.unbind("onchange");
    var row = element.parent().parent().clone();
    row.appendTo(element.parent().parent().parent().parent().find('#reworked-parts-list'));
    row.children().find('input')[0].value = '';
    row.find(`option[value="${selectedValue}"]`).remove();
}

function orderFiltred(element) {
    element = $(element);
    var data = element.val();

    var orderDropDown = element.parent().parent().find('select[name^="Order"]');
    var partDropDown = element.parent().parent().find('select[name^="Part"]');
    var delNumberDropDown = element.parent().parent().find('select[name^="DeliveryNumber"]');
    var chargeNumberDropDown = element.parent().parent().find('select[name^="ChargeNumber"]');

    $.ajax({
        type: "POST",
        //url: "@(Url.Action("GetAllOrdersBySupplier"))",
        url: "/Home/GetAllOrdersBySupplier",
        data: { "supplierId": data },
        beforeSend: function () {
            orderDropDown.attr("disabled", true);
        },
        success: function (result) {
            var orderDropDownOptions = orderDropDown;
            orderDropDownOptions.find('option').not(':first').remove();
            partDropDown.find('option').not(':first').remove();
            delNumberDropDown.find('option').not(':first').remove();
            chargeNumberDropDown.find('option').not(':first').remove();

            if (result !== null && result !== undefined) {
                $.each(result.viewModel,
                    function (text, value) {
                        orderDropDownOptions.append(
                            $('<option></option>').val(value.Value).html(value.Text)
                        );
                    });
                orderDropDownOptions.show();
            }
            orderDropDown.attr("disabled", false);
            removeCriteriaWhenChangingOrder(element);
        }
    });
}

function filterChildDropdown(el) {
    var orderAttributeId = $(el).find(":selected").attr('data-orderAttributeId');
    var selectedValueId = el.value;
    var orderId = $("#OrderId").val();

    $.ajax({
        type: "POST",
        url: "/Home/GetChildrenOrderAttribute",
        data: { orderAttributeId, selectedValueId, orderId },
        success: function (result) {
            if (result === "") {
                RemoveDisabledQuantityAndCriteria(el);
            }

            if ($(el).parent().hasClass(`order-attribute-${orderAttributeId}`)) {
                $(el).parent().find('div[class^="order-attribute"]').remove();
                $(el).parent().append(result);
            } else {
                $(el).parent().append(result);
            }

            $('html, body').animate({
                scrollTop: $(document).height()
            }), 3000;
        }
    });
}

function GetOrderAttribute(element) {
    element = $(element);
    var data = element.val();

    $.ajax({
        type: "POST",
        url: "/Home/GetOrderAttribute",
        data: { "orderId": data },
        success: function (html) {
            var attrDiv = element.closest('.col-lg-4').find('.attribute-div');
            attrDiv.empty();
            attrDiv.append(html);

            $('html, body').animate({
                scrollTop: $(document).height()
            }), 3000;
        }
    });
}

function RemoveDisabledQuantityAndCriteria(element) {
    element = $(element);
    var inputChecked = element.closest(".attribute-div").parent().find('input[name^="CheckedQuantity"]');
    inputChecked.attr("disabled", false);

    var inputTime = element.closest(".attribute-div").parent().find('#input-time');
    inputTime.attr("disabled", false);

    var blockedPartsDropDown = element.closest(".attribute-div").parent().parent().find('select[name^="BlockedPart"]').parent().parent();
    blockedPartsDropDown.removeAttr("hidden");

    var reworkedPartsDropDown = element.closest(".attribute-div").parent().parent().find('select[name^="ReworkedPart"]').parent().parent();
    reworkedPartsDropDown.removeAttr("hidden");
}

function GetDataByOrder(element) {
    element = $(element);
    var data = element.val();

    var blockedPartsDropDown = element.parent().parent().parent().find('select[name^="BlockedPart"]');

    $.ajax({
        type: "POST",
        //url: "@(Url.Action("GetBlockedPartsCriteria"))",
        url: "/Home/GetBlockedPartsCriteria",
        data: { "orderId": data },
        beforeSend: function () {
            blockedPartsDropDown.attr("disabled", true);
        },
        success: function (result) {
            var blockedPartsCriteriaDropDown = blockedPartsDropDown;
            blockedPartsCriteriaDropDown.find('option').not(':first').remove();

            if (result !== null && result !== undefined) {
                $.each(result.viewModel,
                    function (text, value) {
                        blockedPartsCriteriaDropDown.append(
                            $('<option></option>').val(value.Value).html(value.Text)
                        );
                    });
                if (result.viewModel.length > 0) {
                    blockedPartsCriteriaDropDown.show();
                }
            }
            blockedPartsDropDown.attr("disabled", false);
        }
    });

    var reworkedPartsDropDown = element.parent().parent().parent().find('select[name^="ReworkedPart"]');

    $.ajax({
        type: "POST",
        //url: "@(Url.Action("GetReworkedPartsCriteria"))",
        url: "/Home/GetReworkedPartsCriteria",
        data: { "orderId": data },
        beforeSend: function () {
            reworkedPartsDropDown.attr("disabled", true);
        },
        success: function (result) {
            var reworkedPartsCriteriaDropDown = reworkedPartsDropDown;
            reworkedPartsCriteriaDropDown.find('option').not(':first').remove();

            if (result !== null && result !== undefined) {
                $.each(result.viewModel,
                    function (text, value) {
                        reworkedPartsCriteriaDropDown.append(
                            $('<option></option>').val(value.Value).html(value.Text)
                        );
                    });
                if (result.viewModel.length > 0) {
                    reworkedPartsCriteriaDropDown.show();
                }
            }
            reworkedPartsDropDown.attr("disabled", false);
        }
    });
}

function removeRow(element) {
    $(element).parent().parent().parent().parent().remove();
    var a = $("#reports").children('.row.mt-5').last().attr("id", "last-report");
}

function submitReports(errorMessage, returnUrl) {
    var reports = $('#reports').children();

    var models = new Array();
    var errors = false;

    reports.each(function () {
        var object = new Object();
        var attributeArray = new Array();
        var element = $(this);

        object.supplierId = $(element.find('[name^="Supplier"]')[0]).val();
        object.OrderId = $(element.find('[name^="Order"]')[0]).val();
        object.CheckedQuantity = $(element.find("#CheckedQuantity")[0]).val();
        object.InputTime = $(element.find("#input-time")[0]).val();

        var attributes = $(element.find('div[class^="order-attribute"]'));
        attributes.each(function () {
            var attributeClassName = $(this).attr('class');
            var attributeId = attributeClassName.slice(attributeClassName.length - 1);

            var select = $(this).children('select');
            var attributeValueId = select.find(":selected").val();

            var objectAttribute = new Object();
            objectAttribute.AttributeId = attributeId;
            objectAttribute.AttributeValueId = attributeValueId;
            attributeArray.push(objectAttribute);
        });

        object.PostedAttributes = attributeArray;

        object.WorkShiftId = $('#WorkShiftId').val();
        object.ReportDate = $("#DateId").val();

        if (object.WorkShiftId < 1
            || object.CheckedQuantity < 1
            || object.InputTime < 1
            || object.supplierId < 1
            || object.OrderId < 1
            || object.ReportDate === "") {

            errors = true;
        }

        var nokCriteria = new Array();
        var reworkedCriteria = new Array();
        element.find("#blocked-parts-list").children(".row").each(function () {
            var selectId = $(this).find('select').val();
            var value = $(this).find('input').val();
            var objectCriteria = new Object();
            objectCriteria.CriteriaId = selectId;
            objectCriteria.Quantity = value;
            nokCriteria.push(objectCriteria);
        });
        element.find("#reworked-parts-list").children(".row").each(function () {
            var selectId = $(this).find("select").val();
            var value = $(this).find("input").val();
            var objectCriteria = new Object();
            objectCriteria.CriteriaId = selectId;
            objectCriteria.Quantity = value;
            reworkedCriteria.push(objectCriteria);
        });
        object.NokCriteria = nokCriteria;
        object.ReworkedCriteria = reworkedCriteria;
        models.push(object);
    });


    if (errors === true) {
        alert(errorMessage);
    } else {
        $.ajax({
            type: "POST",
            url: "/Home/Index",
            data: { "reports": models, "returnUrl": returnUrl },
            success: function (result) {
                $("#ignismyModal").modal("show").delay(2000).fadeOut();
                setTimeout(function () { location.reload(); }, 2000);
            }
        });
    }
}

function removeCriteriaWhenChangingOrder(element) {
    //blocked parts
    var blockedPartsCriteriaDropDown = element.parent().parent().parent().find('select[name^="BlockedPart"]');
    blockedPartsCriteriaDropDown.find('option').not(':first').remove();
    blockedPartsCriteriaDropDown.parent().parent().find('input').val(0);
    var blockedPartsList = element.parent().parent().parent().find('#blocked-parts-list');
    blockedPartsList.find('.row').not(':first').remove();

    var reworkedPartsCriteriaDropDown = element.parent().parent().parent().find('select[name^="ReworkedPart"]');
    reworkedPartsCriteriaDropDown.find('option').not(':first').remove();
    reworkedPartsCriteriaDropDown.parent().parent().find('input').val(0);
    var reworkedPartsList = element.parent().parent().parent().find('#reworked-parts-list');
    reworkedPartsList.find('.row').not(':first').remove();

    //element.parent().parent().find('select[name^="Order"]').hide();
    //element.parent().parent().find('select[name^="DeliveryNumber"]').hide();
    //element.parent().parent().find('select[name^="ChargeNumber"]').hide();
}

function calculatOrderTotal(element) {
    element = $(element);
    var nokInputTotal = parseInt(0);
    var reworkedInputTotal = parseInt(0);
    var blockedInputs = $(element).parents('.row.mt-5').find('#blocked-parts-list').find('input');
    for (let i = 0; i < blockedInputs.length; i++) {
        let val = $(blockedInputs[i]).val();
        if (val === '') {
            val = 0;
        }
        nokInputTotal += parseInt(val);
    }

    var reworkedInputs = $(element).parents('.row.mt-5').find('#reworked-parts-list').find('input');
    for (let r = 0; r < reworkedInputs.length; r++) {
        let rel = $(reworkedInputs[r]).val();
        if (rel === '') {
            rel = 0;
        }
        reworkedInputTotal += parseInt(rel);
    }

    element.parents('.row.mt-5').find('#input-nok').val(nokInputTotal);
    element.parents('.row.mt-5').find('#input-reworked').val(reworkedInputTotal);
    calculateOrderTotalByOkChange(element, nokInputTotal, reworkedInputTotal);
}

function calculateOrderTotalByOkChange(element, nokInput, reworkedInputTotal) {
    element = $(element);
    var checked = element.parents('.row.mt-5').find('#CheckedQuantity');

    if (nokInput === undefined) {
        nokInput = element.parents('.row.mt-5').find('#input-nok').val();
        if (nokInput === undefined || nokInput === '') {
            nokInput = 0;
        }
    }

    if (reworkedInputTotal === undefined) {
        reworkedInputTotal = element.parents('.row.mt-5').find('#input-reworked').val();
        if (reworkedInputTotal === undefined || reworkedInputTotal === '') {
            reworkedInputTotal = 0;
        }
    }

    if (checked.length <= 0) {
        checked = parseInt(0, 10);
    }
    const totalOk = parseInt(checked.val(), 10) - (parseInt(nokInput, 10) + parseInt(reworkedInputTotal, 10));
    element.parents('.row.mt-5').find('#input-ok').val(totalOk);
}