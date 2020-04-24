/// <binding AfterBuild='concat, babel' />
module.exports = function(grunt) {
	
  const sass = require('node-sass')
	
  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    concat: {
		options : {
			sourceMap :true
		},
		site: {
		  src: [
			'src/Bobcat.Web/wwwroot/js/app/viewmodels/**/*.js',
			'src/Bobcat.Web/wwwroot/js/app/objects/**/*.js'						
		  ],
		  dest: 'src/Bobcat.Web/wwwroot/js/site.js',
		},
		resources: {
		  src: [
			'src/Bobcat.Web/wwwroot/lib/jquery/dist/jquery.js', 			
			'src/Bobcat.Web/wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.js', 			
			'src/Bobcat.Web/wwwroot/lib/toastr.js/toastr.min.js', 		
			'src/Bobcat.Web/wwwroot/js/jsmpeg.min.js',
			'src/Bobcat.Web/wwwroot/lib/knockout/knockout-latest.min.js'
		  ],
		  dest: 'src/Bobcat.Web/wwwroot/js/resources.js',
	    },
	},	
	babel: {
		options: {
		  sourceMap: true,
		  presets: ['@babel/preset-env']
		},
		dist: {
		  files: {
			'src/Bobcat.Web/wwwroot/js/site.js': 'src/Bobcat.Web/wwwroot/js/site.js'
		  }
		}
    },	
    uglify: {
      options: {
        banner: '/*! <%= pkg.name %> <%= grunt.template.today("dd-mm-yyyy") %> */\n'
      },
      dist: {
        files: {
          'src/Bobcat.Web/wwwroot/js/site.min.js': ['src/Bobcat.Web/wwwroot/js/site.js'],
		  'src/Bobcat.Web/wwwroot/js/resources.min.js': ['src/Bobcat.Web/wwwroot/js/resources.js']
        }
      }
    },
	sass: {
		options: {                       // Target options
			implementation: sass,
			sourceMap: true
		},			
		dist: {			
			files: {
				'src/Bobcat.Web/wwwroot/css/site.css': 'src/Bobcat.Web/wwwroot/sass/site.scss' // 'destination': 'source'				
		  }
		}
	},
    jshint: {
      files: ['Gruntfile.js', 'src/Bobcat.Web/wwwroot/js/app/**/*.js'],
      options: {
        // options here to override JSHint defaults
        globals: {
          jQuery: true,
          console: true,
          module: true,
          document: true
        },
		'esversion': 6		
      },
	  ignore_warning: {
		  options: {
			'expr': true,
		  },
		  src: ['src/Bobcat.Web/wwwroot/js/app/**/*.js'],
      }
    }
  });
  
  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-jshint');  
  grunt.loadNpmTasks('grunt-contrib-concat');
  grunt.loadNpmTasks('grunt-babel');
  grunt.loadNpmTasks('grunt-sass');

  grunt.registerTask('default', ['concat', 'babel', 'uglify', 'sass']);

};