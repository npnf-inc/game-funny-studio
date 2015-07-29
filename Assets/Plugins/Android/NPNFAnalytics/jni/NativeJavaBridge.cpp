#include <stdlib.h>
#include <jni.h>
#include <android/log.h>

extern "C"
{

JavaVM* java_vm;
JNIEnv* jniEnv;
jclass mCountlyClass;
jobject mActivity;
jobject mCountly;

JNIEnv* getEnv()
{
	return jniEnv;
}

jint JNI_OnLoad(JavaVM* vm, void* reserved)
{
	// use __android_log_print for logcat debugging...
	__android_log_print(ANDROID_LOG_INFO, "JavaBridge", "[%s] Creating java link vm = %p\n", __FUNCTION__, vm);
	java_vm = vm;

	// attach our thread to the java vm; obviously it's already attached but this way we get the JNIEnv..
	java_vm->AttachCurrentThread(&jniEnv, 0);
	JNIEnv* env = getEnv();

	// first we try to find our main activity..
	jclass cls_Activity = env->FindClass("com/unity3d/player/UnityPlayer");
	jfieldID fid_Activity	= env->GetStaticFieldID(cls_Activity, "currentActivity", "Landroid/app/Activity;");
	jobject obj_Activity	= env->GetStaticObjectField(cls_Activity, fid_Activity);
	__android_log_print(ANDROID_LOG_INFO, "JavaBridge", "[%s] Current activity = %p\n", __FUNCTION__, obj_Activity);
	mActivity = env->NewGlobalRef(obj_Activity);
	env->DeleteLocalRef(obj_Activity);

	// Get Countly instance
	jclass clsCountly = env->FindClass("ly/count/android/api/Countly");
	mCountlyClass = (jclass) env->NewGlobalRef(clsCountly);
	env->DeleteLocalRef(clsCountly);

	jmethodID midCountly = env->GetStaticMethodID(mCountlyClass, "sharedInstance", "()Lly/count/android/api/Countly;");
	jobject objCountly	= env->CallStaticObjectMethod(mCountlyClass, midCountly);
	__android_log_print(ANDROID_LOG_INFO, "JavaBridge", "[%s] Countly Instance = %p\n", __FUNCTION__, objCountly);
	mCountly = env->NewGlobalRef(objCountly);
	env->DeleteLocalRef(objCountly);

	return JNI_VERSION_1_6;		// minimum JNI version
}

void _Init(const char* serverUrl, const char* appKey, const char* clientId, const char* accessToken)
{
	JNIEnv* env = getEnv();
	jmethodID mid = env->GetMethodID(mCountlyClass, "init", "(Landroid/content/Context;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V");
	jstring jServerUrl = env->NewStringUTF(serverUrl);
	jstring jAppKey = env->NewStringUTF(appKey);
	jstring jClientId = env->NewStringUTF(clientId);
	jstring jAccessToken = env->NewStringUTF(accessToken);
	env->CallVoidMethod(mCountly, mid, mActivity, jServerUrl, jAppKey, jClientId, jAccessToken);
}

void _UpdateAccessToken(const char* value)
{
	JNIEnv* env = getEnv();
	jmethodID mid = env->GetMethodID(mCountlyClass, "updateAccessToken", "(Ljava/lang/String;)V");
	jstring jAccessToken = env->NewStringUTF(value);
	env->CallVoidMethod(mCountly, mid, jAccessToken);
}

void _Start()
{
	JNIEnv* env = getEnv();
	jmethodID mid = env->GetMethodID(mCountlyClass, "onStart", "()V");
	env->CallVoidMethod(mCountly, mid);
}

void _Stop()
{
	JNIEnv* env = getEnv();
	jmethodID mid = env->GetMethodID(mCountlyClass, "onStop", "()V");
	env->CallVoidMethod(mCountly, mid);
}
    
void _SetSkieUserId(const char* userId)
{
    JNIEnv* env = getEnv();
    jmethodID mid = env->GetMethodID(mCountlyClass, "setConnectionQueueUserId", "(Ljava/lang/String;)V");
    jstring jUserId = env->NewStringUTF(userId);
    env->CallVoidMethod(mCountly, mid, jUserId);
}

void _RecordEvent1(const char* key, int count)
{
	JNIEnv* env = getEnv();
	jmethodID mid = env->GetMethodID(mCountlyClass, "recordEvent", "(Ljava/lang/String;I)V");
	jstring jKey = env->NewStringUTF(key);
	env->CallVoidMethod(mCountly, mid, jKey, count);
}

void _RecordEvent2(const char* key, int count, double sum)
{
	JNIEnv* env = getEnv();
	jmethodID mid = env->GetMethodID(mCountlyClass, "recordEvent", "(Ljava/lang/String;ID)V");
	jstring jKey = env->NewStringUTF(key);
	env->CallVoidMethod(mCountly, mid, jKey, count, sum);
}

void _RecordEvent3(const char* key, const char* segmentation, int count)
{
	JNIEnv* env = getEnv();
	jmethodID mid = env->GetMethodID(mCountlyClass, "recordEvent", "(Ljava/lang/String;Ljava/lang/String;I)V");
	jstring jKey = env->NewStringUTF(key);
	jstring jSeg = env->NewStringUTF(segmentation);
	env->CallVoidMethod(mCountly, mid, jKey, jSeg, count);
}

void _RecordEvent4(const char* key, const char* segmentation, int count, double sum)
{
	JNIEnv* env = getEnv();
	jmethodID mid = env->GetMethodID(mCountlyClass, "recordEvent", "(Ljava/lang/String;Ljava/lang/String;ID)V");
	jstring jKey = env->NewStringUTF(key);
	jstring jSeg = env->NewStringUTF(segmentation);
	env->CallVoidMethod(mCountly, mid, jKey, jSeg, count, sum);
}

} // extern "C"
