define(["utilities"], function (utilities) {

    var url;
    return {
        GeneratePhysicalEnrollInfo: function (paremeter) {
            url = "/CompanyWebsite/physicalExaminationEnroll";
            utilities.CallPostApi(url, paremeter).done(function (data) {
                if (data) {
                    $("#submit-type").modal('toggle');
                }
                //location.replace("index.html");
            }).fail(function() {
                
            });
        },
        GenerateSelectInformation: function (dataVal,id,method) {
            var parameter = {
                ParentId: dataVal
            };
            url = "/CompanyWebsite/regions";
            utilities.CallGetApi(url, parameter).done(function (data) {
                if (data.Total <= 0) {
                    alert("Region信息加载错误，请联系管理员！");
                    return;
                }
                var html = utilities.HandlebarsHelp("#oneSelectInfoHTML", data.Results);
                $("#" + id).append(html);
                if ($("#twoSelectId").val() != 0) {
                    method();
                }
            }).fail(function() {

            });
        }
    }
});