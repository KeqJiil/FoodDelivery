import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { RestaurantsPage } from "./Features/restaurants/restaurants-page/restaurants-page";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RestaurantsPage],
  standalone: true,
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
