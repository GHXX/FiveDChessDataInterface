#include "Main.h"
#include "Resources\SDL2\include\SDL.h"
#include "Resources\SDL2\include\SDL_opengl.h"

#include <windows.h>

bool alreadyRan = false;

extern "C" __declspec(dllexport) void DATAINTERFACE_Init(void)
{
	if (!alreadyRan)
	{
		alreadyRan = true;
		auto hGameWindow = GetActiveWindow();
		MessageBox(hGameWindow, L"Injection successful!", L"5D Chess Data Interface Bridge", MB_OK);
	}
}

extern "C" __declspec(dllexport) void DATAINTERFACE_SetDrawStructPtr()
{

}


//
bool isInited = false;
static float angle = 0.0f;
//SDL_Renderer* renderer = NULL;
/// <summary>
/// Called by injection right before draw-buffers are swapped
/// </summary>
extern "C" __declspec(dllexport) void DATAINTERFACE_OnDrawLastHook(SDL_Window* window)
{
	if (!isInited)
	{
		SDL_Init(SDL_INIT_EVERYTHING);
		isInited = true;
	}
	
	/*
	auto renderer = SDL_GetRenderer(window);
	if (renderer == 0)
	{
		MessageBox(GetActiveWindow(), L"Error", L"renderer broke :(", MB_OK);
		return;
	}*/

	//SDL_Rect pRect;
	//pRect.x = 50;
	//pRect.y = 100;
	//pRect.w = 30;
	//pRect.h = 30;

	//int screenwidth = 1920;
	//int screenheight = 1080;

	//glClearColor(0.0, 0.0, 0.0, 0.0);  //Set the cleared screen colour to black
	//glViewport(0, 0, screenwidth, screenheight);   //This sets up the viewport so that the coordinates (0, 0) are at the top left of the window
	//glMatrixMode(GL_PROJECTION);
	//glLoadIdentity();
	//glOrtho(0, screenwidth, screenheight, 0, -10, 10);
	//glMatrixMode(GL_MODELVIEW);
	//glLoadIdentity();
	//glClear(GL_COLOR_BUFFER_BIT or GL_DEPTH_BUFFER_BIT); //Clear the screen and depth buffer

	//glPushMatrix();  //Make sure our transformations don't affect any other transformations in other code
	//glTranslatef(pRect.x, pRect.y, 0.0f);  //Translate rectangle to its assigned x and y position
	////Put other transformations here
	//glBegin(GL_QUADS);   //We want to draw a quad, i.e. shape with four sides
	//glColor3f(1, 0, 0); //Set the colour to red 
	//glVertex2f(0, 0);            //Draw the four corners of the rectangle
	//glVertex2f(0, pRect.h);
	//glVertex2f(pRect.w, pRect.h);
	//glVertex2f(pRect.w, 0);
	//glEnd();
	//glPopMatrix();
	//glFlush();

    
//    /* Our angle of rotation. */
//    static bool should_rotate = true;
//
//    /*
//     * EXERCISE:
//     * Replace this awful mess with vertex
//     * arrays and a call to glDrawElements.
//     *
//     * EXERCISE:
//     * After completing the above, change
//     * it to use compiled vertex arrays.
//     *
//     * EXERCISE:
//     * Verify my windings are correct here ;).
//     */
//    static GLfloat v0[] = { -25.0f, -25.0f,  25.0f };
//    static GLfloat v1[] = { 25.0f, -25.0f,  25.0f };
//    static GLfloat v2[] = { 25.0f,  25.0f,  25.0f };
//    static GLfloat v3[] = { -25.0f,  25.0f,  25.0f };
//    static GLfloat v4[] = { -25.0f, -25.0f, -25.0f };
//    static GLfloat v5[] = { 25.0f, -25.0f, -25.0f };
//    static GLfloat v6[] = { 25.0f,  25.0f, -25.0f };
//    static GLfloat v7[] = { -25.0f,  25.0f, -25.0f };
//    static GLubyte red[] = { 255,   0,   0, 255 };
//    static GLubyte green[] = { 0, 255,   0, 255 };
//    static GLubyte blue[] = { 0,   0, 255, 255 };
//    static GLubyte white[] = { 255, 255, 255, 255 };
//    static GLubyte yellow[] = { 0, 255, 255, 255 };
//    static GLubyte black[] = { 0,   0,   0, 255 };
//    static GLubyte orange[] = { 255, 255,   0, 255 };
//    static GLubyte purple[] = { 255,   0, 255,   0 };
//
//    /* Clear the color and depth buffers. */
//    //glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
//    /*glDisable(GL_DEPTH_TEST);
//    glClear(GL_DEPTH_BUFFER_BIT);
//    glFlush();*/
//
//
//
//    //glDisable(GL_DEPTH_TEST);
//    //glDepthMask(false);
//    /* We don't want to modify the projection matrix. */
//    glMatrixMode(GL_PROJECTION);
//    glLoadIdentity();
//    glOrtho(0, 0, 0, 0, 1, 1);
//
//    /* Rotate. */
//    glRotatef(angle, 0.0, 1.0, 0.0);
//
//    if (should_rotate) {
//        angle += 1.0f;
//        if (angle > 360.0f) {
//            angle = 0.0f;
//        }
//
//    }
//
//    /* Send our triangle data to the pipeline. */
//    glBegin(GL_TRIANGLES);
//
//    glColor4ubv(red);
//    glVertex3fv(v0);
//    glColor4ubv(green);
//    glVertex3fv(v1);
//    glColor4ubv(blue);
//    glVertex3fv(v2);
//
//    glColor4ubv(red);
//    glVertex3fv(v0);
//    glColor4ubv(blue);
//    glVertex3fv(v2);
//    glColor4ubv(white);
//    glVertex3fv(v3);
//
//    glColor4ubv(green);
//    glVertex3fv(v1);
//    glColor4ubv(black);
//    glVertex3fv(v5);
//    glColor4ubv(orange);
//    glVertex3fv(v6);
//
//    glColor4ubv(green);
//    glVertex3fv(v1);
//    glColor4ubv(orange);
//    glVertex3fv(v6);
//    glColor4ubv(blue);
//    glVertex3fv(v2);
//
//    glColor4ubv(black);
//    glVertex3fv(v5);
//    glColor4ubv(yellow);
//    glVertex3fv(v4);
//    glColor4ubv(purple);
//    glVertex3fv(v7);
//
//    glColor4ubv(black);
//    glVertex3fv(v5);
//    glColor4ubv(purple);
//    glVertex3fv(v7);
//    glColor4ubv(orange);
//    glVertex3fv(v6);
//
//    glColor4ubv(yellow);
//    glVertex3fv(v4);
//    glColor4ubv(red);
//    glVertex3fv(v0);
//    glColor4ubv(white);
//    glVertex3fv(v3);
//
//    glColor4ubv(yellow);
//    glVertex3fv(v4);
//    glColor4ubv(white);
//    glVertex3fv(v3);
//    glColor4ubv(purple);
//    glVertex3fv(v7);
//
//    glColor4ubv(white);
//    glVertex3fv(v3);
//    glColor4ubv(blue);
//    glVertex3fv(v2);
//    glColor4ubv(orange);
//    glVertex3fv(v6);
//
//    glColor4ubv(white);
//    glVertex3fv(v3);
//    glColor4ubv(orange);
//    glVertex3fv(v6);
//    glColor4ubv(purple);
//    glVertex3fv(v7);
//
//    glColor4ubv(green);
//    glVertex3fv(v1);
//    glColor4ubv(red);
//    glVertex3fv(v0);
//    glColor4ubv(yellow);
//    glVertex3fv(v4);
//
//    glColor4ubv(green);
//    glVertex3fv(v1);
//    glColor4ubv(yellow);
//    glVertex3fv(v4);
//    glColor4ubv(black);
//    glVertex3fv(v5);
//
//    glEnd();
//    glFlush();
//    //glEnable(GL_DEPTH_TEST);
//    //glDepthMask(true);
//
//    /*
//    maybe do this to reset stuff
//    */
//glMatrixMode(GL_MODELVIEW);
//glLoadIdentity();
    
glDisable(GL_DEPTH_TEST);
glDisable(GL_CULL_FACE);
glDisable(GL_TEXTURE_2D);
glDisable(GL_LIGHTING);

glMatrixMode(GL_PROJECTION);
glLoadIdentity();
glOrtho(-100, 100, -100, 100, -1, 1);

//glMatrixMode(GL_MODELVIEW);
//glLoadIdentity();
glColor3f(1, 1, 1);
glBegin(GL_QUADS);
glVertex3f(20.0f, 20.0f, 0.0f);
glVertex3f(20.0f, -20.0f, 0.0f);
glVertex3f(-20.0f, -20.0f, 0.0f);
glVertex3f(-20.0f, 20.0f, 0.0f);
glEnd();
}