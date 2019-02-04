import numpy as np
import collections
import random as r

class room:
    def __init__(self, door):
        self.map = np.full((10,10), 1)
        self.map[9,0] = 2
        control = True
        step = 0.
        if door == True:
            for i in range(10):
                self.map[i,4] = 3
                self.map[5,i] = 3
            self.map[5,1] = 1
            self.map[5,7] = 1
            self.map[2,4] = 1
            self.map[7,4] = 1

class vac:
    def __init__(self):
        self.on = True
        self.position = [9,0]
        self.compass = ["north", "east", "south", "west"]
        self.facing = "north"
        self.direction = {
            "north": (-1, 0),
            "south": (1, 0),
            "east": (0, 1),
            "west": (0, -1)
        }

    def isWall(self, env):
        if self.facing == "north":
            if self.position[0] == 0:
                return True
            elif env.map[self.position[0] - 1][self.position[1]] == 3:
                return True
            else:
                return False
        elif self.facing == "south":
            if self.position[0] == 9:
                return True
            elif env.map[self.position[0] + 1][self.position[0]] == 3:
                return True
            else:
                return False
        elif self.facing == "west":
            if self.position[1] == 0:
                return True
            elif env.map[self.position[0]][self.position[1] - 1] == 3:
                return True
            else:
                return False
        elif self.facing == "east":
            if self.position[1] == 9:
                return True
            elif env.map[self.position[0]][self.position[1] + 1] == 3:
                return True
            else:
                return False

    def isDirty(self, env):
        if env.map[self.position[0], self.position[1]] == 1:
            return True
        else:
            return False

    def isHome(self, env):
        if env.map[self.position[0], self.position[1]] == 2:
            return True
        else:
            return False

    def turnOff(self, env):
        self.on = False
        return float(np.count_nonzero(env.map == 0))

    def move(self, env):
        if self.isWall(env) == False:
            self.position[0] = self.position[0] + self.direction[self.facing][0]
            self.position[1] = self.position[1] + self.direction[self.facing][1]

    def clean(self, env):
        if self.isDirty(env) == True:
            env.map[self.position[0], self.position[1]] = 0

    def turn(self, side):
        idx = self.compass.index(self.facing)
        change = 0
        if side == 'l':
            change = -1
        elif side == 'r':
            change = 1
        idx = idx + change
        if idx == 4:
            idx = 0
        elif idx == -1:
            idx = 3
        self.facing = self.compass[idx]

def reflexDet(agent, door = False):
    env = room(door)
    control = True
    step = 0

    while control == True:
        if agent.isHome(env):
            if agent.isWall(env):
                cleanCount = agent.turnOff(env)
                control = False

                if door == True:
                    print("Room with walls")
                print("Deterministic Reflex Agent results:")
                print("Clean spaces:")
                print(cleanCount)
                print("Number of steps:")
                print(step)
                print("Ratio:")
                print(cleanCount / step)
            else:
                agent.move(env)
        if agent.isDirty(env):
            if agent.isWall(env):
                agent.turn('r')
            else:
                agent.clean(env)
        else:
            agent.move(env)
        step = step + 1

def reflexRand(agent, door = False):
    stepTotal = 0
    cleanTotal = 0

    for i in range(50):
        control = True
        step = 0
        env = room(door)

        while control == True:
            if agent.isHome(env):
                if agent.isWall(env):
                    rand = r.random()
                    if (rand  < .33):
                        agent.turn('r')
                    elif (rand < .66 and rand >= .33):
                        agent.turn('l')
                    elif (rand > .90):
                        cleanCount = agent.turnOff(env)
                        stepTotal = stepTotal + step
                        cleanTotal = cleanTotal + cleanCount
                        control = False
                else:
                    agent.move(env)
            elif agent.isDirty(env):
                agent.clean(env)
            else:
                rand = r.random()
                if agent.isWall(env):
                    if rand > .5:
                        agent.turn('l')
                    else:
                        agent.turn('r')
                else:
                    if (rand  < .10):
                        agent.turn('r')
                    elif (rand < .20 and rand >= .10):
                        agent.turn('l')
                    elif (rand > .20):
                        agent.move(env)
            step = step + 1

    if door == True:
        print("Room with walls")
    print("Random Reflex Agent results:")
    print("Average Number of Clean spaces:")
    print(cleanTotal / 50)
    print("Average Number of steps:")
    print(stepTotal / 50.)
    print("Ratio:")
    print(cleanTotal / stepTotal)

def memoryDet(agent, door = False):
    bit1 = False
    bit2 = False
    bit3 = False

    env = room(door)
    step = 0
    control = True

    while control == True:
        if agent.isDirty(env):
            agent.clean(env)
        else:
            if agent.isWall(env):
                if agent.isHome(env):
                    cleanCount = agent.turnOff(env)
                    control = False

                    if door == True:
                        print("Room with walls")
                    print("Deterministic Reflex Agent results:")
                    print("Clean spaces:")
                    print(cleanCount)
                    print("Number of steps:")
                    print(step)
                    print("Ratio:")
                    print(cleanCount / step)
                else:
                    if bit2:
                        agent.turn('l')
                    else:
                        agent.turn('r')
                    bit1 = True
                    bit3 = True
            else:
                if bit1:
                    if bit3:
                        agent.move(env)
                        bit3 = False
                    else:
                        if bit2:
                            agent.turn('l')
                        else:
                            agent.turn('r')
                        bit2 = not bit2
                        bit1 = False
                else:
                    agent.move(env)
        step = step + 1

agent1 = vac()
agent2 = vac()
agent3 = vac()
agent4 = vac()
agent5 = vac()
agent6 = vac()

#reflexDet(agent1)
#reflexDet(agent2, True)
#reflexRand(agent3)
#reflexRand(agent4, True)
memoryDet(agent5)
memoryDet(agent6, True)
