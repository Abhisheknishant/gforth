// this file is in the public domain
%module gles
%insert("include")
%{
#include <GLES3/gl3.h>
#include <GLES3/gl3ext.h>
%}
#define const
%apply float { GLfloat, GLclampf };
%apply long { EGLNativePixmapType }
%apply long long { GLuint64 };
%apply SWIGTYPE * { GLintptr, GLsizeiptr, EGLBoolean };

#define SWIG_FORTH_OPTIONS "no-callbacks"

#if defined(host_os_linux_android) || defined(host_os_linux_androideabi)
#define __ANDROID__
#define ANDROID
#endif
#define GL_APICALL
#define GL_APIENTRY

%include <GLES3/gl3.h>
%include <GLES3/gl3ext.h>
