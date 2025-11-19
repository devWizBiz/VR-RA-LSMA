# VR-RA-LSMA

# Scene Planning / Breakdown for Project

## Assets
- Found some assets from store  
- VR player then prefab  

---

## Scenes

### **0. Login**
- Setup Background  
- Title  
- ID text/input field  
- Remember Me toggle  
- Login and guest access buttons  
- Create code for logging in (maybe a manager)  
- Set up Virtual Keyboard  
- Make XR input (needs work)

---

### **1. Lobby / Main Menu**
- Setup Scene  
- Make a floating main menu: “Help”, “Let’s Start Session” above display game objects  
- Main Menu Buttons:
  - Start Training → Scene 3 (Training)
  - Start Assessment → Scene 2 (Assessment)
  - Progress Report (UI Panel)
  - Settings (optional)
- UI Panels:
  - Welcome Profile Panel  
  - Check-In Assessment Panel (sliders, etc.)  
  - Progress Report Panel (make a Prefab)  
- Create Code  

---

### **2. Assessment (clinic-like assets)**
- Setup Scene  
- Choose Options (button + panel for each—maybe generate via code)
  - Range of Motion Assessment  
  - Movement Accuracy and Consistency Test  
  - Grip Strength Management  
- When done, Progress Report appears (UI Panel; only called if Main Menu selects Start Assessment)  
- Create Code  
- Text field for feedback  

---

### **3. Training**
- Setup Scene (fitness asset)  
- Options (activate via code; “Daily” links to Scene 4):
  - Guided Joint Mobility Exercise  
  - Daily Activity Simulations → Scene 4  
  - Typing Ergonomics Simulations  
  - Guided Strength Routine for Pain Management  
- Create Code  
- Text field for feedback  

---

### **4. Daily Activity Training (Apartment asset)**
- Setup Scene  
- Research jar (collider / rigidbody)  
- Text field for feedback  
- Code  
