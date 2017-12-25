/// <vs BeforeBuild='watch, less, lessc' AfterBuild='default, less, watch' Clean='default' />
module.exports = function (grunt) {

    // Project configuration.
    grunt.initConfig({
        less: {
            //compile
            compile: {
                files: {
                    'assets/css/bootstrap.css': "bower_components/bootstrap/less/bootstrap.less"
                }
                
            },
            custom:{
                    expand:true,
                    cwd:"assets/less",
                    src:['*.less','!*.min.css'],
                    dest:"assets/less",
                    ext:'.css'
                }

        },
        cssmin: {
            css: {
                src: ["assets/css/bootstrap.css"],
                dest: "assets/css/bootstrap.min.css"
            },
            custom:{
                expand:true,
                cwd:"assets/less",
                src: ["*.css","!*.min.css"],
                dest: "assets/css",
                ext:".min.css"
            }
        },
        uglify: {
            builda: {
                files: {
                    "assets/js/bootstrap.min.js": "bower_components/bootstrap/dist/js/bootstrap.js"
                }
            },
            buildb: {
                files: {
                    "assets/js/jquery.min.js": "bower_components/JQuery/dist/jquery.js"
                }
            },
            buildc: {
                files: {
                    "assets/js/require.min.js": "bower_components/requirejs/require.js"
                }
            }
        },
        copy: {
            main: {
                expand:true,
                cwd: "bower_components/bootstrap/fonts/",
                src: ["*"],
                dest: "assets/fonts/"
            },
            select2:{
                expand:true,
                cwd: "bower_components/select2/dist",
                src: ["css/select2.min.css","js/select2.min.js"],
                dest: "assets/"
            },
            bootstrap_datepicker:{
                expand:true,
                cwd:"bower_components/bootstrap-datepicker/dist",
                src:["css/bootstrap-datepicker.min.css","js/bootstrap-datepicker.min.js","locales/bootstrap-datepicker.zh-CN.min.js"],
                dest:"assets/"
            },
            bootstrapValidator:{
                expand:true,
                cwd:"bower_components/bootstrapValidator/dist",
                src:["css/bootstrapValidator.min.css","js/bootstrapValidator.min.js"],
                dest:"assets/"
            },
            handlebars:{
                expand:true,
                cwd:"bower_components/handlebars",
                src:["handlebars.min.js"],
                dest:"assets/js"
            },
            swiper: {
                expand: true,
                cwd: "bower_components/swiper/dist/",
                src: ["css/swiper.min.css", "js/swiper.min.js"],
                dest:"assets/"
            }
        },
        watch: {
            scripts: {
                options: {
                    livereload:true
                },
                files: ["assets/css/*.less"],
                tasks: ["lessc"]
            }
        }
    });

    grunt.loadNpmTasks("grunt-contrib-less");
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    grunt.loadNpmTasks("grunt-contrib-uglify");
    grunt.loadNpmTasks("grunt-contrib-copy");
    grunt.loadNpmTasks("grunt-contrib-watch");

    grunt.registerTask("default", ['less', "cssmin", "uglify", "copy", 'watch']);
    grunt.registerTask("lessc", ['less:custom', 'cssmin:custom']);

};