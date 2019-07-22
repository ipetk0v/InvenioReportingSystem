function uniqId() {
    return Math.round(new Date().getTime() + (Math.random() * 100));
}

function show_alert(message, errorMessage) {
    if (confirm(message)) {
        submitReports(errorMessage);
    } else {
        return false;
    }
}

function addNewReport() {
    var element = $('#last-report');

    var newElement = element.clone();
    newElement.find('select[name^="Customer"]').attr("id", uniqId());
    newElement.find('select[name^="Order"]').attr("id", uniqId());
    newElement.find('select[name^="Order"]').hide();
    newElement.find('select[name^="Part"]').attr("id", uniqId());
    newElement.find('select[name^="Part"]').hide();
    newElement.find('select[name^="DeliveryNumber"]').attr("id", uniqId());
    newElement.find('select[name^="DeliveryNumber"]').hide();
    newElement.find('select[name^="ChargeNumber"]').attr("id", uniqId());
    newElement.find('select[name^="ChargeNumber"]').hide();
    newElement.find('select[name^="BlockedPart"]').attr("id", uniqId());
    newElement.find('select[name^="ReworkedPart"]').attr("id", uniqId());

    var el = newElement.appendTo($('#reports'));
    element.removeAttr('id');

    el.find('#blocked-parts-list').find('.row').not(':first').remove();
    el.find('#reworked-parts-list').find('.row').not(':first').remove();

    el.find('#blocked-parts-list').children().find('input').val(0);
    el.find('#blocked-parts-list').children().find('option').not(':first').remove();
    el.find('#reworked-parts-list').children().find('input').val(0);
    el.find('#reworked-parts-list').children().find('option').not(':first').remove();
    el.find('#CheckedQuantity').val(0);
    el.find('#input-ok').val(0);
    el.find('#input-nok').val(0);
    el.find('#input-reworked').val(0);
}

function addBlockedRow(element) {
    element = $(element);
    element.unbind("onchange");
    var row = element.parent().parent().clone();
    row.appendTo(element.parent().parent().parent().parent().find('#blocked-parts-list'));
    row.children().find('input')[0].value = '';
}

function addReworkRow(element) {
    element = $(element);
    element.unbind("onchange");
    var row = element.parent().parent().clone();
    row.appendTo(element.parent().parent().parent().parent().find('#reworked-parts-list'));
    row.children().find('input')[0].value = '';
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
        //url: "@(Url.Action("GetAllOrdersByCustomer"))",
        url: "/Home/GetAllOrdersByCustomer",
        data: { "customerId": data },
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

function GetChargeNumbers(element) {
    element = $(element);
    var data = element.val();

    var chargeNumberDropDown = element.parent().parent().find('select[name^="ChargeNumber"]');

    $.ajax({
        type: "POST",
        //url: "@(Url.Action("GetChargeNumbers"))",
        url: "/Home/GetChargeNumbers",
        data: { "delNumberId": data },
        beforeSend: function () {
            chargeNumberDropDown.attr("disabled", true);
        },
        success: function (result) {
            chargeNumberDropDown.find('option').not(':first').remove();

            if (result !== null && result !== undefined) {
                $.each(result.viewModel,
                    function (text, value) {
                        chargeNumberDropDown.append(
                            $('<option></option>').val(value.Value).html(value.Text)
                        );
                    });
                if (result.viewModel.length > 0) {
                    chargeNumberDropDown.show();
                }
            }
            chargeNumberDropDown.attr("disabled", false);
        }
    });
}

function GetPartsByOrder(element) {
    element = $(element);
    var data = element.val();

    var partsDropDown = element.parent().parent().find('select[name^="Part"]');

    $.ajax({
        type: "POST",
        //url: "@(Url.Action("GetDeliveryNumbers"))",
        url: "/Home/GetOrderParts",
        data: { "orderId": data },
        beforeSend: function () {
            partsDropDown.attr("disabled", true);
        },
        success: function (result) {
            partsDropDown.find('option').not(':first').remove();

            if (result !== null && result !== undefined) {
                $.each(result.viewModel,
                    function (text, value) {
                        partsDropDown.append(
                            $('<option></option>').val(value.Value).html(value.Text)
                        );
                    });
                if (result.viewModel.length > 0) {
                    partsDropDown.show();
                }
            }
            partsDropDown.attr("disabled", false);
        }
    });

    GetDataByOrder(element);
}

function GetDeliveryNumber(element) {
    element = $(element);
    var data = element.val();
    var deliveryNumberDropDown = element.parent().parent().find('select[name^="DeliveryNumber"]');

    $.ajax({
        type: "POST",
        //url: "@(Url.Action("GetDeliveryNumbers"))",
        url: "/Home/GetDeliveryNumbers",
        data: { "partId": data },
        beforeSend: function () {
            deliveryNumberDropDown.attr("disabled", true);
        },
        success: function (result) {
            var delNumberDropDown = deliveryNumberDropDown;
            delNumberDropDown.find('option').not(':first').remove();

            if (result !== null && result !== undefined) {
                $.each(result.viewModel,
                    function (text, value) {
                        delNumberDropDown.append(
                            $('<option></option>').val(value.Value).html(value.Text)
                        );
                    });
                if (result.viewModel.length > 0) {
                    delNumberDropDown.show();
                }
            }
            deliveryNumberDropDown.attr("disabled", false);
        }
    });
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

function submitReports(errorMessage) {
    var reports = $('#reports').children();

    var models = new Array();
    var errors = false;

    reports.each(function () {
        var object = new Object();
        var element = $(this);

        object.CustomerId = $(element.find('[name^="Customer"]')[0]).val();
        object.OrderId = $(element.find('[name^="Order"]')[0]).val();
        object.PartId = $(element.find('[name^="Part"]')[0]).val();
        object.DeliveryNumberId = $(element.find('[name^="DeliveryNumber"]')[0]).val();
        object.ChargeNumberId = $(element.find('[name^="ChargeNumber"]')[0]).val();
        object.CheckedQuantity = $(element.find("#CheckedQuantity")[0]).val();

        object.WorkShiftId = $('#WorkShiftId').val();
        object.ReportDate = $("#DateId").val();

        if (object.WorkShiftId < 1
            || object.CheckedQuantity < 1
            || object.CustomerId < 1
            || object.OrderId < 1
            || object.PartId < 1
            || object.ReportDate === "") {

            errors = true;
        }

        if ($(element.find('[name^="DeliveryNumber"]'))[0].options.length > 1 && object.DeliveryNumberId < 1) {
            errors = true;
        }

        if ($(element.find('[name^="ChargeNumber"]'))[0].options.length > 1 && object.ChargeNumberId < 1) {
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
            //url: "@(Url.Action("Index","Home"))",
            url: "/Home/Index",
            data: { "reports": models },
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