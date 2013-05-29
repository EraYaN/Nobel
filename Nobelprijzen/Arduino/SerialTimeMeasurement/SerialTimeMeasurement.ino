bool running;
bool button1State;
bool lastButton1State;
bool button1Done;
bool button2State;
bool lastButton2State;
bool button2Done;
int button1Pin = 2;
int button2Pin = 3;

void setup(){
	pinMode(button1Pin,INPUT_PULLUP);
	pinMode(button2Pin,INPUT_PULLUP);
	pinMode(13,OUTPUT);
	Serial.begin(115200);
	//Serial.println("Started. Ready. 's' to start, 'r' to reset.");
	running = false;
	digitalWrite(13,running);
}

void loop(){
	digitalWrite(13,running);
	if(Serial.available()){
		char byte;
		byte = Serial.read();
		if(byte=='s'){
			//Serial.println("Started");
			button1State=HIGH;
			button2State=HIGH;
			lastButton1State=HIGH;
			lastButton2State=HIGH;
			button1Done = false;
			button2Done = false;
			running = true;
		}
		if(byte=='r'){
			//Serial.println("Reset");
			running = false;
		}
	}
	if(running){
		button1State = (digitalRead(button1Pin)!=LOW);
		button2State = (digitalRead(button2Pin)!=LOW);
		if(lastButton1State != button1State && button1State==LOW && button1Done==false){
			Serial.print("A");
			button1Done = true;
		}
		if(lastButton2State != button2State && button2State==LOW && button2Done==false){
			Serial.print("B");
			button2Done = true;
		}
		lastButton1State = button1State;
		lastButton2State = button2State;
	}
	if(button1Done&&button2Done)
		running = false;
}